using System;

namespace LinkUs.Core.Connection
{
    public interface IConnectionListener<out TConnection> where TConnection : IConnection
    {
        event Action<TConnection> ConnectionEstablished;
        event Action<TConnection> ConnectionLost;

        void StartListening();
        void StopListening();
    }
}