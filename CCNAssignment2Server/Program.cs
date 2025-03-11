using CCNAssignment2Server;

Console.WriteLine("Simple server has started!");
Server srvr = new Server();

srvr.Start(); // a synchronous call

srvr.Stop();

Console.WriteLine("Simple server has finished!");