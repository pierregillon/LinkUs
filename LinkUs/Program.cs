using System;
using System.Net;

namespace LinkUs
{
    class Program
    {
        static void Main(string[] args)
        {
            var server = new Server(new IPEndPoint(IPAddress.Any, 9000));
            server.Start();
            WriteLine("Server started. Waiting for incoming connections.");
            while (Console.ReadLine() != "exit") { }
            WriteLine("Closing connections...");
            server.Stop();
            WriteLine("Server shutdown.");
        }

        // ----- Utils
        private static void WriteLine(string value)
        {
            Console.WriteLine($"[{DateTime.Now}] {value}");
        }
    }
}