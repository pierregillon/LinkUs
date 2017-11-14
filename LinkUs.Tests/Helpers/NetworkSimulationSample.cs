using System;
using System.Net.Sockets;
using System.Threading;
using LinkUs.Core.Connection;

namespace LinkUs.Tests.Helpers
{
    public class NetworkSimulationSample : IDisposable
    {
        private readonly ManualResetEvent _manualSetEvent = new ManualResetEvent(false);

        public SocketConnection SocketConnection { get; set; }
        public Socket NetworkSimulationClient { get; set; }

        public void WaitForOperation()
        {
            _manualSetEvent.WaitOne(50);
        }
        public void SetOperationCompleted()
        {
            _manualSetEvent.Set();
        }

        public void Dispose()
        {
            SocketConnection.Close();
            NetworkSimulationClient.Close();
            _manualSetEvent?.Dispose();
        }
    }
}