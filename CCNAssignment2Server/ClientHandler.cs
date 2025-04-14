using System.Collections.Concurrent;

namespace CCNAssignment2Server;

public class ClientHandler
{
    public StreamWriter Writer { get; set; }
    public BlockingCollection<string> OutgoingMessages { get; } = new();
}