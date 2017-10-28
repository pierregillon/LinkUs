using System;
using System.Net;
using LinkUs.Core;

namespace LinkUs
{
    class Program
    {
        private static readonly Connector _connector = new Connector();

        static void Main(string[] args)
        {
            _connector.PackageReceived += ConnectorOnPackageReceived;
            _connector.Listen(new IPEndPoint(IPAddress.Any, 9000));
            Console.WriteLine("* Listening for clients...");
            while (Console.ReadLine() != "exit") ;
            Console.WriteLine("* Closing connections...");
            _connector.Close();
            Console.WriteLine("* Closed.");
        }

        private static void ConnectorOnPackageReceived(Package package)
        {
            Console.WriteLine(package);
            _connector.SendDataAsync(package);
        }
    }
}