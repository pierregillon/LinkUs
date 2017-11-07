using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;

namespace LinkUs
{
    public class Server
    {
        private readonly PackageRouter _packageRouter;
        private readonly SocketConnectionListener _connectionListener;
        private readonly List<ClientId> _clients = new List<ClientId>();

        // ----- Constructor
        public Server(IPEndPoint endPoint)
        {
            _packageRouter = new PackageRouter();
            _packageRouter.ClientConnected += PackageRouterOnClientConnected;
            _packageRouter.ClientDisconnected += PackageRouterOnClientDisconnected;
            _packageRouter.TargettedServerPackageReceived += PackageRouterOnTargettedServerPackageReceived;

            _connectionListener = new SocketConnectionListener(endPoint);
            _connectionListener.ConnectionEstablished += ConnectionListenerOnConnectionEstablished;
        }

        // ----- Public methods
        public void Start()
        {
            _connectionListener.StartListening();
        }
        public void Stop()
        {
            _connectionListener.StopListening();
            _packageRouter.Close();
            _clients.Clear();
        }

        // ----- Callbacks
        private void ConnectionListenerOnConnectionEstablished(SocketConnection connection)
        {
            _packageRouter.Connect(connection);
        }
        private void PackageRouterOnClientConnected(ClientId clientId)
        {
            WriteLine($"* Client '{clientId}' connected.");
            _clients.Add(clientId);
        }
        private void PackageRouterOnClientDisconnected(ClientId clientId)
        {
            _clients.Remove(clientId);
            WriteLine($"* Client '{clientId}' disconnected.");
        }
        private void PackageRouterOnTargettedServerPackageReceived(Package package)
        {
            var jsonSerializer = new JsonSerializer();
            var commandLine = jsonSerializer.Deserialize<MessageDescriptor>(package.Content);
            if (commandLine.CommandName == typeof(ListConnectedClient).Name) {
                var value = _clients.Select(x => new ConnectedClient() {
                    Id = x.ToString(),
                    MachineName = Environment.MachineName,
                    Ip = "192.168.1.1"
                }).ToArray();
                var packageResponse = package.CreateResponsePackage(jsonSerializer.Serialize(value));
                _packageRouter.SendPackage(packageResponse);
            }
            else {
                var responsePackage = package.CreateResponsePackage(jsonSerializer.Serialize("Invalid command"));
                _packageRouter.SendPackage(responsePackage);
            }
        }

        // ----- Utils
        private static void WriteLine(string value)
        {
            Console.WriteLine($"[{DateTime.Now}] {value}");
        }
    }
}