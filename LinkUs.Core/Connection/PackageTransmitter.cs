using System;

namespace LinkUs.Core.Connection
{
    public class PackageTransmitter
    {
        private readonly IConnection _connection;
        public event EventHandler<Package> PackageReceived;
        public event EventHandler Closed;

        // ----- Constructor
        public PackageTransmitter(IConnection connection)
        {
            _connection = connection;
            _connection.DataReceived += ConnectionOnDataReceived;
            _connection.Closed += ConnectionOnClosed;
        }

        // ----- Public methods
        public void Send(Package package)
        {
            var bytes = package.ToByteArray();
            _connection.SendAsync(bytes);
        }
        public void Close()
        {
            _connection.Close();
            _connection.DataReceived -= ConnectionOnDataReceived;
            _connection.Closed -= ConnectionOnClosed;
        }

        // ----- Event callbacks
        private void ConnectionOnDataReceived(byte[] bytes)
        {
            var package = Package.Parse(bytes);
            PackageReceived?.Invoke(this, package);
        }
        private void ConnectionOnClosed()
        {
            Closed?.Invoke(this, EventArgs.Empty);
        }
    }
}