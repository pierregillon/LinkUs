using System;

namespace LinkUs.Core.Connection
{
    public interface IConnection
    {
        event Action<byte[]> DataReceived;
        event Action<int> DataSent;
        event Action Closed;

        void SendAsync(byte[] data);
        void Close();
    }
}