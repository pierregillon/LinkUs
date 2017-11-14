using System;
using System.Threading;
using LinkUs.Core.Connection;

namespace LinkUs.Tests.Helpers
{
    public class ConnectedSocketConnectionSample : IDisposable
    {
        private readonly ManualResetEvent _manualSetEvent = new ManualResetEvent(false);

        public SocketConnection Client1 { get; set; }
        public SocketConnection Client2 { get; set; }

        public void WaitForOperation()
        {
            _manualSetEvent.WaitOne(1000);
        }
        public void SetOperationCompleted()
        {
            _manualSetEvent.Set();
        }
        public void Dispose()
        {
            Client1.Close();
            Client2.Close();
            _manualSetEvent.Dispose();
        }
    }
}