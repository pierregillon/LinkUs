using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public event Action<Package> TargettedServerPackageReceived;

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
        public void SendPackage(Package package)
        {
            var packageTransmitter = _activeTransmitter[package.Destination];
            packageTransmitter.Send(package);
        }

        // ----- Event callbacks
        private void PackageTransmitterOnPackageReceived(object sender, Package package)
        {
            var packageTransmitter = (PackageTransmitter) sender;
            var clientId = _activeTransmitter.Single(x => x.Value == packageTransmitter).Key;
            package.ChangeSource(clientId);
            Route(package);
        }
        private void PackageTransmitterOnClosed(object sender, EventArgs eventArgs)
        {
            var packageTransmitter = (PackageTransmitter) sender;
            var entry = _activeTransmitter.Single(x => x.Value == packageTransmitter);
            _activeTransmitter.Remove(entry);
            ClientDisconnected?.Invoke(entry.Key);
        }

        // ----- Internal logics
        private void Route(Package package)
        {
            Console.WriteLine($"[{DateTime.Now}] Package routed : {package}");
            if (!Equals(package.Destination, ClientId.Server)) {
                SendPackage(package);
            }
            else if (Equals(package.Source, package.Destination)) {
                Console.WriteLine($"Client '{package.Source}' is trying to send package to himself.");
            }
            else {
                TargettedServerPackageReceived?.Invoke(package);
            }
        }
    }
}