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
    private IDatabaseGatewayFacade _databaseGatewayFacade;

    public Server()
    {
        IPAddress ipAddress = IPAddress.Loopback;
        _tcpListener = new TcpListener(ipAddress, 4444);
        _databaseGatewayFacade = new DatabaseGatewayFacade();
        Console.WriteLine("Listening....");
    }

    public void Start()
    {
        _tcpListener.Start();
        while (true)
        {
            Socket socket = _tcpListener.AcceptSocket();
            Task.Run(() => InteractWithClient(socket)); // Handle clients concurrently
        }
    }

    public void Stop()
    {
        _tcpListener.Stop();
    }

    private void InteractWithClient(Socket socket)
    {
        using NetworkStream stream = new NetworkStream(socket);
        using StreamReader reader = new StreamReader(stream, Encoding.UTF8);
        using StreamWriter writer = new StreamWriter(stream, Encoding.UTF8) { AutoFlush = true };

        Console.WriteLine("Client connected.");

        try
        {
            // Send an initial message
            var initialMessage = new Message("1. Mary had a little lamb", 1, 2, 3);
            string jsonResponse = JsonSerializer.Serialize(initialMessage);
            writer.WriteLine(jsonResponse);

            string clientJson;
            while (socket.Connected && (clientJson = reader.ReadLine()) != null)
            {
                try
                {
                    Message? clientMessage = JsonSerializer.Deserialize<Message>(clientJson);
                    if (clientMessage == null || string.IsNullOrWhiteSpace(clientMessage.Action))
                    {
                        Console.WriteLine("Received invalid or empty request.");
                        continue;
                    }

                    Console.WriteLine("Processing request: " + clientMessage.Action);
                    string responseContent = ProcessClientMessage(clientMessage);
                    var responseMessage = new Message(responseContent, 1, 2, 3);

                    string responseJson = JsonSerializer.Serialize(responseMessage);
                    writer.WriteLine(responseJson);
                }
                catch (JsonException)
                {
                    Console.WriteLine("Received invalid JSON.");
                    writer.WriteLine(JsonSerializer.Serialize(new { Error = "Invalid JSON format." }));
                }
            }
        }
        catch (IOException e)
        {
            Console.WriteLine("ERROR: " + e.Message);
        }
        catch (Exception e)
        {
            Console.WriteLine("Unexpected error: " + e.Message);
        }

        socket.Close();
        Console.WriteLine("Client disconnected.");
    }

    private string ProcessClientMessage(Message clientMessage)
    {
        Console.WriteLine("Client says: " + clientMessage.Action);
        switch (clientMessage.Action)
        {
            case "AddBook":
                _databaseGatewayFacade.AddBook(_databaseGatewayFacade.FindBook(clientMessage.BookId));
                break;
            case "AddMember":
                _databaseGatewayFacade.AddMember(new Member(1, "TestMember"));
                break;
            case "CreateLoan":
                _databaseGatewayFacade.CreateLoan(null);
                break;
            case "EndLoan":
                _databaseGatewayFacade.EndLoan(clientMessage.LoanId, clientMessage.BookId);
                break;
            case "FindBook":
                _databaseGatewayFacade.FindBook(clientMessage.BookId);
                break;
            case "FindLoan":
                _databaseGatewayFacade.FindLoan(clientMessage.LoanId, clientMessage.BookId);
                break;
            case "FindMember":
                _databaseGatewayFacade.FindMember(clientMessage.MemberId);
                break;
            case "GetAllBooks":
                _databaseGatewayFacade.GetAllBooks();
                break;
            case "GetAllMembers":
                _databaseGatewayFacade.GetAllMembers();
                break;
            case "GetCurrentLoans":
                _databaseGatewayFacade.GetCurrentLoans();
                break;
            case "InitialiseDatabase":
                _databaseGatewayFacade.InitialiseDatabase();
                break;
            case "RenewLoan":
                _databaseGatewayFacade.RenewLoan(null);
                break;
            default:
                throw new Exception("Unknown request type.");
        }

        return "";
    }
}
