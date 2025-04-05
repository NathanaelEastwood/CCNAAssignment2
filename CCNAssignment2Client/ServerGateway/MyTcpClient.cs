using System.IO;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Text.Json;
using EntityLayer;

namespace CCNAssignment2.ServerGateway;

public static class MyTcpClient
{
    private static readonly TcpClient _tcpClient;
    private static NetworkStream _stream;
    private static StreamReader _reader;
    private static StreamWriter _writer;

    static MyTcpClient()
    {
        _tcpClient = new TcpClient();
        if (!Connect("localhost", 4444))
        {
            throw new Exception("Connection failed");
        }
    }
    
    private static bool Connect(string url, int portNumber)
    {
        try
        {
            _tcpClient.Connect(url, portNumber);
            _stream = _tcpClient.GetStream();
            _reader = new StreamReader(_stream, System.Text.Encoding.UTF8);
            _writer = new StreamWriter(_stream, System.Text.Encoding.UTF8);
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
            return false;
        }
        return true;
    }

    public static ResponseMessageDTO WriteToServer(ClientMessageDTO clientMessageDto)
    {
        Console.WriteLine($"Writing line with {clientMessageDto.Action}");

        // Serialize and send the message
        string jsonMessage = JsonSerializer.Serialize(clientMessageDto);
        _writer.WriteLine(jsonMessage); // Send message with newline
        _writer.Flush(); // Ensure data is sent immediately

        // Read the response from the server
        string responseLine = _reader.ReadLine(); // Assuming _reader is a StreamReader
        Console.WriteLine($"Received response: {responseLine}");

        // Deserialize the response
        ResponseMessageDTO response = JsonSerializer.Deserialize<ResponseMessageDTO>(responseLine);
        return response;
    }

}
