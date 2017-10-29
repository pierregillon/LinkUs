using System;

namespace LinkUs.Core
{
    public class PackageConnector
    {
        private readonly IConnection _connection;
        public event EventHandler<Package> PackageReceived;
        public event EventHandler Closed;

        public PackageConnector(IConnection connection)
        {
            _connection = connection;
            _connection.DataReceived += ConnectionOnDataReceived;
            _connection.Closed += () => Closed?.Invoke(this, EventArgs.Empty);
        }

        public void Send(Package package)
        {
            var bytes = package.ToByteArray();
            _connection.SendAsync(bytes);
        }
        public void Close()
        {
            _connection.Close();
        }

        private void ConnectionOnDataReceived(byte[] bytes)
        {
            var package = Package.Parse(bytes);
            PackageReceived?.Invoke(this, package);
        }
    }
}