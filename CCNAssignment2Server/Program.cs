using CCNAssignment2Server;

Console.WriteLine("Simple server has started!");
Server srvr = new Server();

srvr.Start();

Console.CancelKeyPress += (sender, e) =>
{
    e.Cancel = true;
    Console.WriteLine("\nStopping server...");
    srvr.Stop();
};

Console.WriteLine("Press Ctrl+C to exit.");
Thread.Sleep(Timeout.Infinite);