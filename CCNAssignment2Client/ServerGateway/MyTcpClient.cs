using System.IO;
using System.Net.Sockets;
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

    public static void WriteToServer()
    {
        if (Connect("localhost", 4444))
        {
            Console.WriteLine("Writing line");
            _writer.Write(JsonSerializer.Serialize(new Message("test", 1, 2, 3)));
            _writer.Flush();
        }
    }
    
    private static void InteractWithServer()
    {
        if (Connect("localhost", 4444))
        {
            string serverInput = _reader.ReadLine();
            while (!serverInput.Equals("."))
            {
                string response = ProcessServerMessage(serverInput);
                _writer.WriteLine(response);
                _writer.Flush();
                serverInput = _reader.ReadLine();
            }
        }
        else
        {
            throw new Exception("Failed to connect to server");
        }
    }

    private static string ProcessServerMessage(string serverInput)
    {
        Console.WriteLine("Server says: " + serverInput);
        if (serverInput.Contains("Mary had"))
        {
            return "2. It's fleece was white as snow";
        }

        if (serverInput.Contains("Everywhere"))
        {
            return "4. The lamb was sure to go";
        }

        return "";
    }
}
