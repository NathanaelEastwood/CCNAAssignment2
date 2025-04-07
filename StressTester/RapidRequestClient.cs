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

        // Helper methods to create test data
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