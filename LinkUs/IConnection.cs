using System;

namespace LinkUs
{
    public interface IConnection
    {
        event Action<byte[]> DataReceived;
        event Action<int> DataSent;
        event Action Closed;

        void SendAsync(byte[] data);
        void StartContinuousReceive();
        void Close();
    }
}