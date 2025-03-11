using System.Net;
using System.Net.Sockets;
using System.Text.Json;
using EntityLayer;

namespace CCNAssignment2Server;

public class Server
{
    private TcpListener _tcpListener;

    public Server()
    {
        IPAddress ipAddress = IPAddress.Loopback;
        _tcpListener = new TcpListener(ipAddress, 4444);
        Console.WriteLine("Listening....");
    }

    public void Start()
    {
        _tcpListener.Start();
        Socket s = _tcpListener.AcceptSocket();
        InteractWithClient(s);
    }

    public void Stop()
    {
        _tcpListener.Stop();
    }

    private void InteractWithClient(Socket s)
    {
        NetworkStream stream = new NetworkStream(s);
        StreamReader reader = new StreamReader(stream, System.Text.Encoding.UTF8);
        StreamWriter writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
        
        string? x = reader.ReadLine();

        while (x != null)
        {
            Message? message = JsonSerializer.Deserialize<Message>(x);
            writer.WriteLine(message?.Action);

            x = reader.ReadLine();
        }
    }
}