using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using DatabaseGateway;
using Entities;
using EntityLayer;
using Org.BouncyCastle.Security.Certificates;
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

            string clientJson;
            while (socket.Connected && (clientJson = reader.ReadLine()) != null)
            {
                try
                {
                    ClientMessageDTO? clientMessage = JsonSerializer.Deserialize<ClientMessageDTO>(clientJson);
                    if (clientMessage == null || string.IsNullOrWhiteSpace(clientMessage.Action))
                    {
                        Console.WriteLine("Received invalid or empty request.");
                        continue;
                    }

                    Console.WriteLine("Processing request: " + clientMessage.Action);
                    string responseContent = ProcessClientMessage(clientMessage);
                    var responseMessage = new ResponseMessageDTO(responseContent);

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

    private string ProcessClientMessage(ClientMessageDTO clientMessageDto)
    {
        Console.WriteLine("Client says: " + clientMessageDto.Action);
        switch (clientMessageDto.Action)
        {
            case "AddBook":
                if (clientMessageDto.Book != null)
                {
                    _databaseGatewayFacade.AddBook(clientMessageDto.Book);
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action AddBook");
                }
                break;
            case "AddMember":
                if (clientMessageDto.Member != null)
                {
                    _databaseGatewayFacade.AddMember(clientMessageDto.Member);
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action AddMember");
                }
                break;
            case "CreateLoan":
                if (clientMessageDto.Loan != null)
                {
                    _databaseGatewayFacade.CreateLoan(clientMessageDto.Loan);
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action CreateLoan");
                }
                break;
            case "EndLoan":
                if (clientMessageDto is { Loan: not null, Book: not null })
                {
                    _databaseGatewayFacade.EndLoan(clientMessageDto.Loan.ID, clientMessageDto.Book.ID);
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action EndLoan");
                }
                break;
            case "FindBook":
                if (clientMessageDto.BookId != null)
                {
                    _databaseGatewayFacade.FindBook((int)clientMessageDto.BookId);
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action FindBook");
                }

                break;
            case "FindLoan":
                if (clientMessageDto is { BookId: not null, LoanId: not null })
                {
                    _databaseGatewayFacade.FindLoan((int)clientMessageDto.LoanId, (int)clientMessageDto.BookId);
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action FindLoan");
                }

                break;
            case "FindMember":
                if (clientMessageDto.MemberId != null)
                {
                    _databaseGatewayFacade.FindMember((int)clientMessageDto.MemberId);
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action FindMember");
                }
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
                if (clientMessageDto.Loan != null)
                {
                    _databaseGatewayFacade.RenewLoan(clientMessageDto.Loan);
                }
                else
                {
                    throw new Exception("Incorrect parameters given for action RenewLoan");
                }
                break;
            default:
                throw new Exception("Unknown request type.");
        }

        return "";
    }
}
