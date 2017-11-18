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
            var container = BuildNewContainer();
            var server = container.GetInstance<Server>();
            server.Start(new IPEndPoint(IPAddress.Any, 9000));
            WriteLine("Server started. Waiting for incoming connections.");
            while (Console.ReadLine() != "exit") { }
            WriteLine("Closing connections...");
            server.Stop();
            WriteLine("Server shutdown.");
        }

        // ----- Utils
        private static Ioc BuildNewContainer()
        {
            var ioc = new Ioc();
            ioc.RegisterSingle<Server>();
            ioc.RegisterSingle<PackageRouter>();
            ioc.RegisterSingle<SocketConnectionListener>();
            ioc.RegisterSingle<Connector>();
            ioc.RegisterSingle(new SocketAsyncOperationPool(20));
            return ioc;
        }
        private static void WriteLine(string value)
        {
            Console.WriteLine($"[{DateTime.Now}] {value}");
        }
    }
}