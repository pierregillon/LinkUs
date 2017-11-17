using System;
using System.Net;

namespace LinkUs.Core.Connection
{
    public interface IConnectionListener<out TConnection> where TConnection : IConnection
    {
        event Action<TConnection> ConnectionEstablished;

        void StartListening(IPEndPoint endPoint );
        void StopListening();
    }
}