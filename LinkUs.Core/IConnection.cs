using System;

namespace LinkUs.Core
{
    public interface IConnection
    {
        event Action<byte[]> DataReceived;
        event Action<int> DataSent;
        event Action Closed;

        void Connect(string host, int port);
        void SendAsync(byte[] data);
        void Close();
    }
}