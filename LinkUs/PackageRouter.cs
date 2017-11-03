using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinkUs.CommandLine;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;

namespace LinkUs
{
    public class PackageRouter
    {
        private readonly IDictionary<ClientId, PackageTransmitter> _activeTransmitter = new ConcurrentDictionary<ClientId, PackageTransmitter>();
        public event Action<ClientId> ClientConnected;
        public event Action<ClientId> ClientDisconnected;

        // ----- Public methods
        public void Connect(IConnection connection)
        {
            var packageTransmitter = new PackageTransmitter(connection);
            packageTransmitter.Closed += PackageTransmitterOnClosed;
            packageTransmitter.PackageReceived += PackageTransmitterOnPackageReceived;
            var newClientId = ClientId.New();
            _activeTransmitter.Add(newClientId, packageTransmitter);
            ClientConnected?.Invoke(newClientId);
        }
        public void Close()
        {
            foreach (var packageTransmitter in _activeTransmitter.Values) {
                packageTransmitter.Close();
            }
        }

        // ----- Event callbacks
        private void PackageTransmitterOnPackageReceived(object sender, Package package)
        {
            var packageTransmitter = (PackageTransmitter) sender;
            var clientId = _activeTransmitter.Single(x => x.Value == packageTransmitter).Key;
            package.ChangeSource(clientId);
            ProcessPackage(package);
        }
        private void PackageTransmitterOnClosed(object sender, EventArgs eventArgs)
        {
            var packageTransmitter = (PackageTransmitter) sender;
            var entry = _activeTransmitter.Single(x => x.Value == packageTransmitter);
            _activeTransmitter.Remove(entry);
            ClientDisconnected?.Invoke(entry.Key);
        }

        // ----- Internal logics
        private void SendPackage(Package package)
        {
            var packageTransmitter = _activeTransmitter[package.Destination];
            packageTransmitter.Send(package);
        }
        private void ProcessPackage(Package package)
        {
            if (!Equals(package.Destination, ClientId.Server)) {
                Console.WriteLine($"* Package routed {package}");
                SendPackage(package);
            }
            else if (Equals(package.Source, package.Destination)) {
                Console.WriteLine($"Client '{package.Source}' is trying to send package to himself.");
            }
            else {
                var jsonSerializer = new JsonSerializer();
                var commandLine = jsonSerializer.Deserialize<MessageDescriptor>(package.Content);
                if (commandLine.Name == typeof(ListRemoteClients).Name) {
                    var clients = _activeTransmitter.Keys;
                    var value = string.Join(Environment.NewLine, clients.Select(x => x.ToString()));
                    var packageResponse = package.CreateResponsePackage(jsonSerializer.Serialize(value));
                    SendPackage(packageResponse);
                }
                else {
                    var responsePackage = package.CreateResponsePackage(jsonSerializer.Serialize("Invalid command"));
                    SendPackage(responsePackage);
                }
            }
        }
    }
}