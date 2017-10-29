using System;

namespace LinkUs.Core
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
            _connection.Closed += () => Closed?.Invoke(this, EventArgs.Empty);
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
        }

        // ----- Event callbacks
        private void ConnectionOnDataReceived(byte[] bytes)
        {
            var package = Package.Parse(bytes);
            PackageReceived?.Invoke(this, package);
        }
    }
}