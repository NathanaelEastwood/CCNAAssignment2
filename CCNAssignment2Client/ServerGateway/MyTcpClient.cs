﻿using System.IO;
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
    public static event Action<ResponseMessageDTO> OnMessageReceived;

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
            _writer = new StreamWriter(_stream, System.Text.Encoding.UTF8) { AutoFlush = true };

            // Start listening to server messages
            Task.Run(ListenForMessages); 
        }
        catch (Exception e)
        {
            Console.WriteLine("Exception: " + e.Message);
            return false;
        }
        return true;
    }

    private static async Task ListenForMessages()
    {
        try
        {
            while (true)
            {
                var line = await _reader.ReadLineAsync();
                if (line == null) break;

                Console.WriteLine($"[Server] {line}");
                var message = JsonSerializer.Deserialize<ResponseMessageDTO>(line);

                if (message != null)
                    OnMessageReceived?.Invoke(message);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in listener: {ex.Message}");
        }
    }

    
    public static void WriteToServer(ClientMessageDTO clientMessageDto)
    {
        Console.WriteLine($"Writing line with {clientMessageDto.Action}");
        string jsonMessage = JsonSerializer.Serialize(clientMessageDto);
        _writer.WriteLine(jsonMessage);
    }
}
