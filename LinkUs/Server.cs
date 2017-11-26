using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using LinkUs.Client.ClientInformation;
using LinkUs.Commands;
using LinkUs.Core;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using LinkUs.Core.Packages;
using LinkUs.Responses;

namespace LinkUs
{
    public class Server
    {
        private readonly PackageRouter _packageRouter;
        private readonly SocketConnectionListener _connectionListener;
        private readonly IDictionary<ClientId, ClientBasicInformation> _clients = new Dictionary<ClientId, ClientBasicInformation>();

        // ----- Constructor
        public Server(PackageRouter packageRouter, SocketConnectionListener connectionListener)
        {
            _packageRouter = packageRouter;
            _packageRouter.ClientConnected += PackageRouterOnClientConnected;
            _packageRouter.ClientDisconnected += PackageRouterOnClientDisconnected;
            _packageRouter.TargettedServerPackageReceived += PackageRouterOnTargettedServerPackageReceived;

            _connectionListener = connectionListener;
            _connectionListener.ConnectionEstablished += ConnectionListenerOnConnectionEstablished;
        }

        // ----- Public methods
        public void Start(IPEndPoint endPoint)
        {
            _connectionListener.StartListening(endPoint);
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
            WriteLine($"Server process : Client '{clientId}' connected.");
        }
        private void PackageRouterOnClientDisconnected(ClientId clientId)
        {
            _clients.Remove(clientId);
            WriteLine($"Server process : Client '{clientId}' disconnected.");
        }
        private void PackageRouterOnTargettedServerPackageReceived(Package package)
        {
            var jsonSerializer = new JsonCommandSerializer();
            var commandLine = jsonSerializer.Deserialize<CommandDescriptor>(package.Content);
            if (commandLine.CommandName == typeof(SetStatus).Name) {
                var command = jsonSerializer.Deserialize<SetStatus>(package.Content);
                if (command.Status == "Provider") {
                    var packageResponse = package.CreateResponsePackage(jsonSerializer.Serialize(new GetBasicInformation()));
                    _packageRouter.SendPackage(packageResponse);
                }
                else {
                    _clients.Remove(package.Source);
                }
            }
            else if (commandLine.CommandName == typeof(ClientBasicInformation).Name) {
                var information = jsonSerializer.Deserialize<ClientBasicInformation>(package.Content);
                _clients.Add(package.Source, information);
                WriteLine($"Server process : Client {package.Source.ToShortString()} updated its information.");
            }
            else if (commandLine.CommandName == typeof(ListConnectedClient).Name) {
                var value = _clients.Select(x => new ConnectedClient() {
                        Id = x.Key.ToString(),
                        MachineName = x.Value.MachineName,
                        OperatingSystem = x.Value.OperatingSystem,
                        UserName = x.Value.UserName,
                        PublicIp = x.Value.PublicIp
                    })
                    .ToArray();
                var packageResponse = package.CreateResponsePackage(jsonSerializer.Serialize(value));
                _packageRouter.SendPackage(packageResponse);
            }
        }

        // ----- Utils
        private static void WriteLine(string value)
        {
            Console.WriteLine($"[{DateTime.Now}] {value}");
        }
    }
}