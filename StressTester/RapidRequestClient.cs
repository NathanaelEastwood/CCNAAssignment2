using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Entities;
using EntityLayer;

namespace CCNAssignment2Client
{
    public class RapidRequestClient : IDisposable
    {
        private readonly string _host;
        private readonly int _port;
        private readonly int _maxConcurrentRequests;
        private readonly SemaphoreSlim _semaphore;
        private readonly ConcurrentDictionary<int, Stopwatch> _requestTimers;
        private readonly Random _random = new Random();
        private readonly ConcurrentQueue<ConnectionInfo> _connectionPool;
        private readonly object _poolLock = new object();
        private int _reconnectAttempts = 0;
        private const int MaxReconnectAttempts = 3;

        private class ConnectionInfo : IDisposable
        {
            public TcpClient Client { get; }
            public StreamReader Reader { get; }
            public StreamWriter Writer { get; }
            public bool IsInUse { get; set; }

            public ConnectionInfo(TcpClient client, StreamReader reader, StreamWriter writer)
            {
                Client = client;
                Reader = reader;
                Writer = writer;
                IsInUse = false;
            }

            public void Dispose()
            {
                Client.Dispose();
                Reader.Dispose();
                Writer.Dispose();
            }
        }

        public RapidRequestClient(string host = "127.0.0.1", int port = 4444, int maxConcurrentRequests = 10)
        {
            _host = host;
            _port = port;
            _maxConcurrentRequests = maxConcurrentRequests;
            _semaphore = new SemaphoreSlim(maxConcurrentRequests);
            _requestTimers = new ConcurrentDictionary<int, Stopwatch>();
            _connectionPool = new ConcurrentQueue<ConnectionInfo>();
        }

        private ConnectionInfo CreateConnection()
        {
            var client = new TcpClient();
            client.Connect(_host, _port);
            var stream = client.GetStream();
            var reader = new StreamReader(stream);
            var writer = new StreamWriter(stream) { AutoFlush = true };
            return new ConnectionInfo(client, reader, writer);
        }

        private ConnectionInfo GetConnection()
        {
            // Try to get an existing connection from the pool
            while (_connectionPool.TryDequeue(out var connection))
            {
                if (!connection.IsInUse && connection.Client.Connected)
                {
                    connection.IsInUse = true;
                    return connection;
                }
                connection.Dispose();
            }

            // Create a new connection if none available
            return CreateConnection();
        }

        private void ReturnConnection(ConnectionInfo connection)
        {
            if (connection.Client.Connected)
            {
                connection.IsInUse = false;
                _connectionPool.Enqueue(connection);
            }
            else
            {
                connection.Dispose();
            }
        }

        public async Task<RequestResult> SendRequestAsync(ClientMessageDTO message)
        {
            await _semaphore.WaitAsync();
            var requestId = Environment.TickCount;
            var timer = new Stopwatch();
            _requestTimers[requestId] = timer;
            timer.Start();

            ConnectionInfo? connection = null;
            try
            {
                while (_reconnectAttempts < MaxReconnectAttempts)
                {
                    try
                    {
                        connection = GetConnection();
                        var jsonMessage = JsonSerializer.Serialize(message);
                        await connection.Writer.WriteLineAsync(jsonMessage);

                        var response = await connection.Reader.ReadLineAsync();
                        timer.Stop();

                        if (response == null)
                        {
                            _reconnectAttempts++;
                            connection.Dispose();
                            connection = null;
                            continue;
                        }

                        var responseDto = JsonSerializer.Deserialize<ResponseMessageDTO>(response);
                        return new RequestResult
                        {
                            RequestId = requestId,
                            StatusCode = responseDto?.ResponseCode ?? -1,
                            ResponseTime = timer.ElapsedMilliseconds,
                            IsSuccessful = responseDto?.ResponseCode == 1,
                            ErrorMessage = responseDto?.ErrorMessage
                        };
                    }
                    catch (IOException)
                    {
                        _reconnectAttempts++;
                        connection?.Dispose();
                        connection = null;
                        if (_reconnectAttempts >= MaxReconnectAttempts)
                            throw;
                        await Task.Delay(100 * _reconnectAttempts);
                    }
                }

                throw new Exception("Failed to connect after multiple attempts");
            }
            catch (Exception ex)
            {
                timer.Stop();
                return new RequestResult
                {
                    RequestId = requestId,
                    StatusCode = -1,
                    ResponseTime = timer.ElapsedMilliseconds,
                    IsSuccessful = false,
                    ErrorMessage = ex.Message
                };
            }
            finally
            {
                if (connection != null)
                {
                    ReturnConnection(connection);
                }
                _semaphore.Release();
                _requestTimers.TryRemove(requestId, out _);
            }
        }

        public async Task<List<RequestResult>> SendBulkRequestsAsync(ClientMessageDTO message, int count)
        {
            var tasks = new List<Task<RequestResult>>();
            for (int i = 0; i < count; i++)
            {
                tasks.Add(SendRequestAsync(message));
            }

            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }

        public async Task<List<RequestResult>> SendBulkRandomRequestsAsync(int count)
        {
            var tasks = new List<Task<RequestResult>>();
            for (int i = 0; i < count; i++)
            {
                var message = CreateRandomRequest();
                tasks.Add(SendRequestAsync(message));
            }

            var results = await Task.WhenAll(tasks);
            return results.ToList();
        }
        // Helper methods to create test data

        public ClientMessageDTO CreateRandomRequest()
        {
            var useCase = _random.Next(0, 10);

            ClientMessageDTO request;
            
            switch (useCase)
            {
                case 0:
                    request = CreateAddBookRequest();
                    break;
                case 1:
                    request = CreateAddMemberRequest();
                    break;
                case 2:
                    request = CreateAddLoanRequest();
                    break;
                case 3:
                    request = CreateFindBookRequest(_random.Next(0, 6000));
                    break;
                case 4:
                    request = CreateFindLoanRequest(_random.Next(0, 5000), _random.Next(0, 5000));
                    break;
                case 5:
                    request = CreateFindMemberRequest(_random.Next(0, 5000));
                    break;
                case 6:
                    request = CreateEndLoanRequest(_random.Next(0, 5000), _random.Next(0, 5000));
                    break;
                case 7:
                    request = CreateGetAllBooksRequest();
                    break;
                case 8:
                    request = CreateGetAllMembersRequest();
                    break;
                case 9:
                    request = CreateGetCurrentLoansRequest();
                    break;
                case 10:
                    request = CreateRenewLoanRequest();
                    break;
                default:
                    throw new Exception("Incorrect option specified.");
            }

            return request;
        }

        private ClientMessageDTO CreateRenewLoanRequest()
        {
            return new ClientMessageDTO
            {
                Action = "RenewLoan",
                Loan =  new Loan(_random.Next(0, 6000), new Member(_random.Next(0, 6000), "TestMember"),
                    new Book(_random.Next(0, 6000), "TestAuth", "TestBook", "A123", BookState.Available), DateTime.Now,
                    DateTime.Now)
            };
        } 
        
        private ClientMessageDTO CreateGetCurrentLoansRequest()
        {
            return new ClientMessageDTO
            { 
                Action = "GetCurrentLoans"
            };
        }
        
        private ClientMessageDTO CreateGetAllMembersRequest()
        {
            return new ClientMessageDTO
            {
                Action = "GetAllMembers"
            };
        }

        private ClientMessageDTO CreateGetAllBooksRequest()
        {
            return new ClientMessageDTO
            {
                Action = "GetAllBooks"
            };
        }
        
        public ClientMessageDTO CreateAddLoanRequest()
        {
            return new ClientMessageDTO
            {
                Action = "CreateLoan",
                LoanId = _random.Next(100000, 999999),
                Loan = new Loan(_random.Next(100000, 999999), new Member(_random.Next(0, 6000), "TestMember"),
                    new Book(_random.Next(0, 6000), "TestAuth", "TestBook", "A123", BookState.Available), DateTime.Now,
                    DateTime.Now)
            };
        }
        public ClientMessageDTO CreateAddBookRequest(string title = "Test Book", string author = "Test Author")
        {
            return new ClientMessageDTO
            {
                Action = "AddBook",
                Book = new Book(_random.Next(100000, 999999), author, title, $"ISBN{_random.Next(100000, 999999)}", BookState.Available)
            };
        }

        public ClientMessageDTO CreateAddMemberRequest(string name = "Test Member")
        {
            return new ClientMessageDTO
            {
                Action = "AddMember",
                Member = new Member(_random.Next(100000, 999999), name)
            };
        }

        public ClientMessageDTO CreateFindBookRequest(int bookId)
        {
            return new ClientMessageDTO
            {
                Action = "FindBook",
                BookId = bookId
            };
        }

        private ClientMessageDTO CreateFindMemberRequest(int memberId)
        {
            return new ClientMessageDTO
            {
                MemberId = memberId,
                Action = "FindMember"
            };
        }

        private ClientMessageDTO CreateFindLoanRequest(int bookId, int memberId)
        {
            return new ClientMessageDTO
            {
                Action = "FindLoan",
                BookId = bookId,
                MemberId = memberId
            };
        }

        private ClientMessageDTO CreateEndLoanRequest(int memberId, int bookId)
        {
            return new ClientMessageDTO
            {
                MemberId = memberId,
                BookId = bookId,
                Action = "EndLoan"
            };
        }
    
        public void Dispose()
        {
            _semaphore.Dispose();
            while (_connectionPool.TryDequeue(out var connection))
            {
                connection.Dispose();
            }
        }
    }

    public class RequestResult
    {
        public int RequestId { get; set; }
        public int StatusCode { get; set; }
        public long ResponseTime { get; set; }
        public bool IsSuccessful { get; set; }
        public string? ErrorMessage { get; set; }
    }
} 