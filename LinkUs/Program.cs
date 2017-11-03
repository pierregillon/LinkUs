using System;
using System.Net;
using LinkUs.Core;
using LinkUs.Core.Connection;

namespace LinkUs
{
    class Program
    {
        static void Main(string[] args)
        {
            var packageRouter = new PackageRouter();
            packageRouter.ClientConnected += clientId => {
                WriteLine($"* Client '{clientId}' connected.");
            };
            packageRouter.ClientDisconnected += clientId => {
                WriteLine($"* Client '{clientId}' disconnected.");
            };
            var connectionListener = new SocketConnectionListener(new IPEndPoint(IPAddress.Any, 9000));
            connectionListener.ConnectionEstablished += connection => {
                packageRouter.Connect(connection);
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