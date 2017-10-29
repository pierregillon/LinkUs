using System;
using System.Net;

namespace LinkUs
{
    class Program
    {
        static void Main(string[] args)
        {
            var packageRouter = new PackageRouter();
            var connectionListener = new SocketConnectionListener(new IPEndPoint(IPAddress.Any, 9000));
            connectionListener.ConnectionEstablished += connection => {
                connection.StartContinuousReceive();
                packageRouter.Add(connection);
            };
            connectionListener.StartListening();

            WriteLine("* Server started. Waiting for incoming connections.");
            while (Console.ReadLine() != "exit") { }
            WriteLine("* Closing connections...");
            connectionListener.StopListening();
            packageRouter.Close();
            WriteLine("* Server shutdown.");
        }

        private static void WriteLine(string value)
        {
            Console.WriteLine($"[{DateTime.Now}] {value}");
        }
    }
}