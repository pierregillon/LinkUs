using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public class SocketConnectionListener : IConnectionListener<SocketConnection>
    {
        private readonly IConnectionFactory<SocketConnection> _connectionFactory;
        private Socket _listenSocket;
        private readonly Queue<SocketAsyncEventArgs> _acceptSocketOperations = new Queue<SocketAsyncEventArgs>();

        public event Action<SocketConnection> ConnectionEstablished;

        // ----- Constructors
        public SocketConnectionListener(IConnectionFactory<SocketConnection> connectionFactory)
        {
            _connectionFactory = connectionFactory;
            for (int i = 0; i < 5; i++) {
                var args = new SocketAsyncEventArgs();
                args.Completed += AcceptEventCompleted;
                _acceptSocketOperations.Enqueue(args);
            }
        }

        // ----- Public methods
        public void StartListening(IPEndPoint endPoint)
        {
            if (_listenSocket != null) {
                throw new Exception("Already listening");
            }
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(endPoint);
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
            _listenSocket = null;
        }

        // ----- Internal logics
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
                RecycleSocketAsyncEventArgs(acceptSocketEventArgs);
                return;
            }
            if (acceptSocketEventArgs.SocketError != SocketError.Success) {
                RecycleSocketAsyncEventArgs(acceptSocketEventArgs);
                throw new Exception("error");
            }

            StartAcceptNextConnection();
            ProcessNewSocket(acceptSocketEventArgs.AcceptSocket);
            RecycleSocketAsyncEventArgs(acceptSocketEventArgs);
        }
        private void ProcessNewSocket(Socket socket)
        {
            var socketConnection = _connectionFactory.Create(socket);
            ConnectionEstablished?.Invoke(socketConnection);
        }
        private void RecycleSocketAsyncEventArgs(SocketAsyncEventArgs acceptSocketEventArgs)
        {
            acceptSocketEventArgs.AcceptSocket = null;
            _acceptSocketOperations.Enqueue(acceptSocketEventArgs);
        }
    }
}