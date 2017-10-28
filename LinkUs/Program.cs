using System;
using System.Net;

namespace LinkUs
{
    class Program
    {
        static void Main(string[] args)
        {
            var connector = new Connector();
            connector.PackageReceived += package => {
                WriteLine($"{package}");
            };
            connector.ClientConnected += clientId => {
                WriteLine($"Client '{clientId}' connected.");
            };
            connector.ClientDisconnected += clientId => {
                WriteLine($"Client '{clientId}' disconnected.");
            };

            var server = new Server(connector);

            server.Start(new IPEndPoint(IPAddress.Any, 9000));
            WriteLine("* Server started. Waiting for incoming connections.");
            while (Console.ReadLine() != "exit") { }
            WriteLine("* Closing connections...");
            server.Shutdown();
            WriteLine("* Server shutdown.");
        }

        private static void WriteLine(string value)
        {
            Console.WriteLine($"[{DateTime.Now}] {value}");
        }
    }
}