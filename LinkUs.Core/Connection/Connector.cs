using System;
using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public class Connector
    {
        private readonly SocketAsyncOperationPool _asyncOperationPool;

        // ----- Constructor
        public Connector(SocketAsyncOperationPool asyncOperationPool)
        {
            _asyncOperationPool = asyncOperationPool;
        }

        // ----- Public methods
        public SocketConnection Connect(string host, int port)
        {
            var socket = BuildDefaultSocket();
            socket.Connect(host, port);
            return new SocketConnection(_asyncOperationPool, socket);
        }
        public SocketConnection ConnectFromActiveSocket(Socket socket)
        {
            if (socket.Connected == false) {
                throw new Exception("Cannot create connection from unactive socket.");
            }
            return new SocketConnection(_asyncOperationPool, socket);
        }

        // ----- Utils
        private static Socket BuildDefaultSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}