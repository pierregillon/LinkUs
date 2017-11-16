using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public class SocketConnectionListener : IConnectionListener<SocketConnection>
    {
        private const int ASYNC_OPERATION_COUNT = 20;
        private readonly SocketAsyncOperationPool _pool = new SocketAsyncOperationPool(ASYNC_OPERATION_COUNT);

        private readonly Socket _listenSocket;
        private readonly Queue<SocketAsyncEventArgs> _acceptSocketOperations = new Queue<SocketAsyncEventArgs>();

        public event Action<SocketConnection> ConnectionEstablished;

        public SocketConnectionListener(IPEndPoint endPoint)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(endPoint);

            for (int i = 0; i < 5; i++) {
                var args = new SocketAsyncEventArgs();
                args.Completed += AcceptEventCompleted;
                _acceptSocketOperations.Enqueue(args);
            }
        }

        public void StartListening()
        {
            _listenSocket.Listen(10);

            StartAcceptNextConnection();
        }
        public void StopListening()
        {
            try {
                _listenSocket.Shutdown(SocketShutdown.Both);
            }
            // Stopping asynchrone operation can throw exception.
            catch (SocketException) { }

            _listenSocket.Close();
        }

        private void StartAcceptNextConnection()
        {
            var acceptSocketEventArgs = _acceptSocketOperations.Dequeue();
            var isPending = _listenSocket.AcceptAsync(acceptSocketEventArgs);
            if (isPending == false) {
                ProcessAccept(acceptSocketEventArgs);
            }
        }
        private void AcceptEventCompleted(object sender, SocketAsyncEventArgs acceptSocketEventArgs)
        {
            ProcessAccept(acceptSocketEventArgs);
        }
        private void ProcessAccept(SocketAsyncEventArgs acceptSocketEventArgs)
        {
            if (acceptSocketEventArgs.SocketError == SocketError.OperationAborted) {
                return;
            }
            if (acceptSocketEventArgs.SocketError != SocketError.Success) {
                throw new Exception("error");
            }

            StartAcceptNextConnection();

            ConnectionEstablished?.Invoke(new SocketConnection(_pool, acceptSocketEventArgs.AcceptSocket));

            acceptSocketEventArgs.AcceptSocket = null;
            _acceptSocketOperations.Enqueue(acceptSocketEventArgs);
        }
    }
}