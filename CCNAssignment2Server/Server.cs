using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Collections.Concurrent;
using DatabaseGateway;
using Entities;
using EntityLayer;
using UseCase;

namespace CCNAssignment2Server;
public class Server
{
    private readonly TcpListener _tcpListener;
    private readonly IDatabaseGatewayFacade _databaseGatewayFacade;
    private readonly CancellationTokenSource _cts = new();
    
    // Request queue for handling concurrent client requests
    private readonly BlockingCollection<RequestItem> _requestQueue = new BlockingCollection<RequestItem>();
    private readonly List<Task> _workerTasks = new List<Task>();
    private readonly int _workerCount = Environment.ProcessorCount; // Number of worker threads based on CPU cores
    
    // Collection of all active streams
    private readonly ConcurrentBag<StreamWriter> _connectedClients = [];
    // Track active clients with their locks for synchronized access
    private readonly ConcurrentDictionary<StreamWriter, SemaphoreSlim> _clientLocks = new();

    // Combined response and input object,
    private class RequestItem
    {
        public ClientMessageDTO ClientMessage { get; set; }
        public TaskCompletionSource<ResponseMessageDTO> ResponseSource { get; set; }
    }

    public Server()
    {
        IPAddress ipAddress = IPAddress.Loopback;
        _tcpListener = new TcpListener(ipAddress, 4444);
        _databaseGatewayFacade = new DatabaseGatewayFacade();
    }

    public void Start()
    {
        _tcpListener.Start();
        Console.WriteLine("Server started. Listening for clients...");
        
        // Start worker threads to process the request queue
        for (int i = 0; i < _workerCount; i++)
        {
            _workerTasks.Add(Task.Run(() => ProcessRequestsWorker(_cts.Token)));
        }
        Console.WriteLine($"Started {_workerCount} worker threads to process requests");
        
        _databaseGatewayFacade.InitialiseDatabase();
        
        Task.Run(() => AcceptClientsLoopAsync(_cts.Token));
    }

    public void Stop()
    {
        Console.WriteLine("Stopping server...");
        _cts.Cancel();
        _requestQueue.CompleteAdding(); // Signal to workers that no more items will be added
        _tcpListener.Stop();
        
        // Wait for all workers to complete (with a timeout)
        Task.WaitAll(_workerTasks.ToArray(), 5000);
    }

    private async Task AcceptClientsLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client = await _tcpListener.AcceptTcpClientAsync(token);
                Console.WriteLine("Client connected.");

                _ = Task.Run(() => InteractWithClient(client), token);
            }
        }
        catch (ObjectDisposedException)
        {
            // Listener was stopped. Expected on shutdown.
        }
        catch (Exception ex)
        {
            Console.WriteLine("Error in AcceptClientsLoopAsync: " + ex.Message);
        }
    }

    private async Task InteractWithClient(TcpClient tcpClient)
    {
        using (tcpClient)
        {
            await using NetworkStream stream = tcpClient.GetStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            await using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8);
            writer.AutoFlush = true;
            
            // Add the client with its own lock object for synchronized access
            var clientLock = new SemaphoreSlim(1, 1);
            _connectedClients.Add(writer);
            _clientLocks[writer] = clientLock;
            
            try
            {
                while (await reader.ReadLineAsync() is { } clientJson)
                {
                    try
                    {
                        var clientMessage = JsonSerializer.Deserialize<ClientMessageDTO>(clientJson);
                        if (clientMessage == null || string.IsNullOrWhiteSpace(clientMessage.Action))
                        {
                            Console.WriteLine("Received invalid or empty request.");
                            continue;
                        }

                        Console.WriteLine($"Queueing request: {clientMessage.Action}");

                        // Create a TaskCompletionSource to get the result asynchronously
                        var responseSource = new TaskCompletionSource<ResponseMessageDTO>();

                        // Add the request to the queue
                        if (!_requestQueue.IsAddingCompleted)
                        {
                            _requestQueue.Add(new RequestItem
                            {
                                ClientMessage = clientMessage,
                                ResponseSource = responseSource
                            });

                            // Wait for the response from the worker thread
                            var responseContent = await responseSource.Task;
                            string responseJson = JsonSerializer.Serialize(responseContent);
                            
                            // Acquire lock before writing to the stream
                            await clientLock.WaitAsync();
                            try
                            {
                                await writer.WriteLineAsync(responseJson);
                            }
                            finally
                            {
                                clientLock.Release();
                            }
                        }
                        else
                        {
                            // Server is shutting down
                            await clientLock.WaitAsync();
                            try
                            {
                                await writer.WriteLineAsync(JsonSerializer.Serialize(
                                    new ResponseMessageDTO { ResponseCode = 0, ErrorMessage = "Server is shutting down" }));
                            }
                            finally
                            {
                                clientLock.Release();
                            }
                        }
                    }
                    catch (JsonException)
                    {
                        Console.WriteLine("Received invalid JSON.");
                        await clientLock.WaitAsync();
                        try
                        {
                            await writer.WriteLineAsync(JsonSerializer.Serialize(
                                new ResponseMessageDTO { ResponseCode = 0, ErrorMessage = "Invalid JSON format." }));
                        }
                        finally
                        {
                            clientLock.Release();
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Request failed");
                        await clientLock.WaitAsync();
                        try
                        {
                            await writer.WriteLineAsync(JsonSerializer.Serialize(
                                new ResponseMessageDTO { ResponseCode = 0, ErrorMessage = "Request failed, try checking the request parameters." }));
                        }
                        finally
                        {
                            clientLock.Release();
                        }
                    }
                }
            }
            catch (IOException e)
            {
                Console.WriteLine("Client connection error: " + e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected error: " + e.Message);
            }
            finally
            {
                // Cleanup client resources
                _clientLocks.TryRemove(writer, out _);
            }

            Console.WriteLine("Client disconnected.");
        }
    }
    
    // This method is to broadcast the updated values of loans/books when a request is received from another client.
    private void BroadcastMessage(ResponseMessageDTO messageDto)
    {
        string messageJson = JsonSerializer.Serialize(messageDto);
        
        // Use a separate task for broadcasting to avoid blocking the worker thread
        Task.Run(async () => 
        {
            foreach (var writer in _connectedClients)
            {
                // Skip disconnected clients
                if (writer == null || !_clientLocks.TryGetValue(writer, out var clientLock))
                    continue;
                
                // Try to acquire the lock, but don't wait indefinitely if stream is busy
                bool lockAcquired = await clientLock.WaitAsync(50);
                if (!lockAcquired)
                {
                    // Client is busy with a request, will try again on next broadcast
                    continue;
                }
                
                try
                {
                    // Check if we can still write to this client
                    await writer.WriteLineAsync(messageJson);
                    await writer.FlushAsync();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error broadcasting to client: {ex.Message}");
                    // Don't remove the client here as it may still be in use
                }
                finally
                {
                    clientLock.Release();
                }
            }
        });
    }

    // Worker method to process requests from the queue
    private void ProcessRequestsWorker(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested && !_requestQueue.IsCompleted)
            {
                // Try to take an item from the queue with a timeout
                if (_requestQueue.TryTake(out RequestItem requestItem, 100))
                {
                    try
                    {
                        Console.WriteLine($"Worker {Environment.CurrentManagedThreadId} processing request: {requestItem.ClientMessage.Action}. Current queue length is: {_requestQueue.Count}");
                        var response = ProcessClientMessage(requestItem.ClientMessage);
                        requestItem.ResponseSource.SetResult(response);
                    }
                    catch (Exception ex)
                    {
                        // Set the exception so the client thread can handle it
                        requestItem.ResponseSource.SetResult(new ResponseMessageDTO 
                        { 
                            ResponseCode = 0, 
                            ErrorMessage = $"Error processing request: {ex.Message}" 
                        });
                        Console.WriteLine($"Error processing request: {ex.Message}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Worker thread error: {ex.Message}");
        }
        finally
        {
            Console.WriteLine("Worker thread exiting");
        }
    }

    private ResponseMessageDTO ProcessClientMessage(ClientMessageDTO clientMessageDto)
    {
        List<Loan> loans;
        switch (clientMessageDto.Action)
        {
            case "AddBook":
                if (clientMessageDto.Book != null)
                {
                    _databaseGatewayFacade.AddBook(clientMessageDto.Book);
                    return new ResponseMessageDTO { ResponseCode = 1 };
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action AddBook");
                }
            case "AddMember":
                if (clientMessageDto.Member != null)
                {
                    _databaseGatewayFacade.AddMember(clientMessageDto.Member);
                    return new ResponseMessageDTO { ResponseCode = 1 };
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action AddMember");
                }
            case "CreateLoan":
                if (clientMessageDto.Loan != null)
                {
                    _databaseGatewayFacade.CreateLoan(clientMessageDto.Loan);
                    List<Loan> newLoans = _databaseGatewayFacade.GetCurrentLoans();
                    List<Book> newBooks = _databaseGatewayFacade.GetAllBooks();
                    BroadcastMessage(new ResponseMessageDTO { ResponseCode = 1, Book = newBooks, Loan = newLoans });
                    return new ResponseMessageDTO { ResponseCode = 1 };
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action CreateLoan");
                }
            case "EndLoan":
                if (clientMessageDto is { MemberId: not null, BookId: not null })
                {
                    _databaseGatewayFacade.EndLoan((int)clientMessageDto.MemberId, (int)clientMessageDto.BookId);
                    List<Loan> newLoans = _databaseGatewayFacade.GetCurrentLoans();
                    List<Book> newBooks = _databaseGatewayFacade.GetAllBooks();
                    BroadcastMessage(new ResponseMessageDTO { ResponseCode = 1, Book = newBooks, Loan = newLoans });
                    return new ResponseMessageDTO
                    {
                        ResponseCode = 1,
                    };
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action EndLoan");
                }
            case "FindBook":
                if (clientMessageDto.BookId != null)
                {
                    Book book = _databaseGatewayFacade.FindBook((int)clientMessageDto.BookId);
                    return new ResponseMessageDTO
                    {
                        ResponseCode = 1,
                        Book = new List<Book> { book }
                    };
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action FindBook");
                }
            case "FindLoan":
                if (clientMessageDto is { BookId: not null, LoanId: not null })
                {
                    Loan loan = _databaseGatewayFacade.FindLoan((int)clientMessageDto.LoanId,
                        (int)clientMessageDto.BookId);
                    return new ResponseMessageDTO
                    {
                        ResponseCode = 1,
                        Loan = new List<Loan> { loan }
                    };
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action FindLoan");
                }
            case "FindMember":
                if (clientMessageDto.MemberId != null)
                {
                    Member member = _databaseGatewayFacade.FindMember((int)clientMessageDto.MemberId);
                    return new ResponseMessageDTO
                    {
                        ResponseCode = 1,
                        Member = new List<Member> { member }
                    };
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action FindMember");
                }
            case "GetAllBooks":
                List<Book> books = _databaseGatewayFacade.GetAllBooks();
                return new ResponseMessageDTO
                {
                    ResponseCode = 1,
                    Book = books
                };
            case "GetAllMembers":
                List<Member> members = _databaseGatewayFacade.GetAllMembers();
                return new ResponseMessageDTO
                {
                    ResponseCode = 1,
                    Member = members
                };
            case "GetCurrentLoans":
                loans = _databaseGatewayFacade.GetCurrentLoans();
                return new ResponseMessageDTO
                {
                    ResponseCode = 1,
                    Loan = loans
                };
            case "InitialiseDatabase":
                _databaseGatewayFacade.InitialiseDatabase();
                return new ResponseMessageDTO
                {
                    ResponseCode = 1
                };
            case "RenewLoan":
                if (clientMessageDto.Loan != null)
                {
                    _databaseGatewayFacade.RenewLoan(clientMessageDto.Loan);
                    var newLoans = _databaseGatewayFacade.GetCurrentLoans();
                    BroadcastMessage(new ResponseMessageDTO { ResponseCode = 1, Loan = newLoans });
                    return new ResponseMessageDTO
                    {
                        ResponseCode = 1
                    };
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action RenewLoan");
                }

                break;
            default:
                throw new Exception("Unknown request type.");
        }
    }
}
