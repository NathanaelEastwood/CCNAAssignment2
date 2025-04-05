using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DatabaseGateway;
using Entities;
using EntityLayer;
using UseCase;

namespace CCNAssignment2Server;
public class Server
{
    private readonly TcpListener _tcpListener;
    private readonly IDatabaseGatewayFacade _databaseGatewayFacade;
    private readonly CancellationTokenSource _cts = new CancellationTokenSource();

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

        Task.Run(() => AcceptClientsLoopAsync(_cts.Token));
    }

    public void Stop()
    {
        Console.WriteLine("Stopping server...");
        _cts.Cancel();
        _tcpListener.Stop();
    }

    private async Task AcceptClientsLoopAsync(CancellationToken token)
    {
        try
        {
            while (!token.IsCancellationRequested)
            {
                TcpClient client = await _tcpListener.AcceptTcpClientAsync();
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
            using NetworkStream stream = tcpClient.GetStream();
            using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
            using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

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

                        Console.WriteLine("Processing request: " + clientMessage.Action);
                        ResponseMessageDTO responseContent = ProcessClientMessage(clientMessage);

                        string responseJson = JsonSerializer.Serialize(responseContent);
                        await writer.WriteLineAsync(responseJson);
                    }
                    catch (JsonException)
                    {
                        Console.WriteLine("Received invalid JSON.");
                        await writer.WriteLineAsync(JsonSerializer.Serialize(new { Error = "Invalid JSON format." }));
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

            Console.WriteLine("Client disconnected.");
        }
    }


    private ResponseMessageDTO ProcessClientMessage(ClientMessageDTO clientMessageDto)
    {
        Console.WriteLine("Client says: " + clientMessageDto.Action);
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
                    return new ResponseMessageDTO { ResponseCode = 1 };
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action CreateLoan");
                }
            case "EndLoan":
                if (clientMessageDto is { Loan: not null, Book: not null })
                {
                    _databaseGatewayFacade.EndLoan(clientMessageDto.Loan.ID, clientMessageDto.Book.ID);
                    return new ResponseMessageDTO { ResponseCode = 1 };
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
                        Book = new List<Book>{book}
                    };
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action FindBook");
                }
            case "FindLoan":
                if (clientMessageDto is { BookId: not null, LoanId: not null })
                {
                    Loan loan = _databaseGatewayFacade.FindLoan((int)clientMessageDto.LoanId, (int)clientMessageDto.BookId);
                    return new ResponseMessageDTO
                    {
                        ResponseCode = 1,
                        Loan = new List<Loan>{loan}
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
                        Member = new List<Member>{member}
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
                List<Loan> loans = _databaseGatewayFacade.GetCurrentLoans();
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
