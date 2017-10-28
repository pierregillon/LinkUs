﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using LinkUs.Core;

namespace LinkUs
{
    public class Connector
    {
        private Socket _listenSocket;
        private readonly Dictionary<ClientId, Socket> _connectedSockets = new Dictionary<ClientId, Socket>();
        private readonly Queue<SocketAsyncEventArgs> _acceptSocketOperations = new Queue<SocketAsyncEventArgs>();
        private readonly Queue<SocketAsyncEventArgs> _receiveSocketOperations = new Queue<SocketAsyncEventArgs>();
        private readonly Queue<SocketAsyncEventArgs> _sendSocketOperations = new Queue<SocketAsyncEventArgs>();

        public event Action<ClientId> ClientConnected;
        protected virtual void OnClientConnected(ClientId clientId)
        {
            ClientConnected?.Invoke(clientId);
        }
        public event Action<Package> PackageReceived;
        protected virtual void OnPackageReceived(Package obj)
        {
            PackageReceived?.Invoke(obj);
        }

        // ----- Constructor

        public Connector()
        {
            for (int i = 0; i < 2; i++) {
                var args = new SocketAsyncEventArgs();
                args.Completed += AcceptEventCompleted;
                _acceptSocketOperations.Enqueue(args);
            }

            for (int i = 0; i < 2; i++) {
                var args = new SocketAsyncEventArgs();
                args.Completed += ReceiveEventCompleted;
                args.SetBuffer(new byte[10000], 0, 10000);
                _receiveSocketOperations.Enqueue(args);
            }

            for (int i = 0; i < 2; i++) {
                var args = new SocketAsyncEventArgs();
                args.Completed += SendEventCompleted;
                args.SetBuffer(new byte[10000], 0, 10000);
                _sendSocketOperations.Enqueue(args);
            }
        }

        // ----- Public methods

        public void Listen(IPEndPoint endPoint)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            _listenSocket.Bind(endPoint);
            _listenSocket.Listen(10);

            StartAcceptNextConnection();
        }
        public void SendDataAsync(Package package)
        {
            var socket = _connectedSockets[package.Destination];
            StartSendData(socket, package.ToByteArray());
        }
        public IEnumerable<ClientId> GetClients()
        {
            return _connectedSockets.Keys.ToArray();
        }
        public void Close()
        {
            try {
                _listenSocket.Shutdown(SocketShutdown.Both);
            }
            // Stopping asynchrone operation can throw exception.
            catch (SocketException) {}

            _listenSocket.Close();

            foreach (var socket in _connectedSockets.Values) {
                socket.Close();
            }

            _connectedSockets.Clear();
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
            if (acceptSocketEventArgs.SocketError != SocketError.Success) {
                throw new Exception("error");
            }
            var clientId = ClientId.New();
            _connectedSockets.Add(clientId, acceptSocketEventArgs.AcceptSocket);
            OnClientConnected(clientId);
            StartAcceptNextConnection();
            StartReceiveData(acceptSocketEventArgs.AcceptSocket);
            _acceptSocketOperations.Enqueue(acceptSocketEventArgs);
        }

        private void StartReceiveData(Socket socket)
        {
            var receiveSocketEventArgs = _receiveSocketOperations.Dequeue();
            receiveSocketEventArgs.AcceptSocket = socket;
            var isPending = receiveSocketEventArgs.AcceptSocket.ReceiveAsync(receiveSocketEventArgs);
            if (!isPending) {
                ProcessReceiveData(receiveSocketEventArgs);
            }
        }
        private void ReceiveEventCompleted(object sender, SocketAsyncEventArgs receiveSocketEventArgs)
        {
            ProcessReceiveData(receiveSocketEventArgs);
        }
        private void ProcessReceiveData(SocketAsyncEventArgs receiveSocketEventArgs)
        {
            if (receiveSocketEventArgs.LastOperation != SocketAsyncOperation.Receive) {
                throw new Exception("bad operation");
            }
            if (receiveSocketEventArgs.SocketError != SocketError.Success) {
                throw new Exception("unsuccessed read");
            }
            var bytesTransferredCount = receiveSocketEventArgs.BytesTransferred;
            var bytesTransferred = receiveSocketEventArgs.Buffer.Take(bytesTransferredCount).ToArray();
            OnPackageReceived(Package.Parse(bytesTransferred));

            StartReceiveData(receiveSocketEventArgs.AcceptSocket);
            receiveSocketEventArgs.AcceptSocket = null;
            _receiveSocketOperations.Enqueue(receiveSocketEventArgs);
        }

        private void StartSendData(Socket socket, byte[] buffer)
        {
            var sendSocketEventArgs = _sendSocketOperations.Dequeue();
            sendSocketEventArgs.AcceptSocket = socket;
            sendSocketEventArgs.SetBuffer(buffer, 0, buffer.Length);
            var isPending = sendSocketEventArgs.AcceptSocket.SendAsync(sendSocketEventArgs);
            if (isPending == false) {
                ProcessSendData(sendSocketEventArgs);
            }
        }
        private void SendEventCompleted(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            ProcessSendData(asyncEventArgs);
        }
        private void ProcessSendData(SocketAsyncEventArgs socketAsyncEventArgs)
        {
            if (socketAsyncEventArgs.LastOperation != SocketAsyncOperation.Send) {
                throw new Exception("bad operation");
            }
            if (socketAsyncEventArgs.SocketError != SocketError.Success) {
                throw new Exception("unsuccessed read");
            }

            var bytesTransferred = socketAsyncEventArgs.BytesTransferred;
            Console.WriteLine($"* {bytesTransferred} bytes sent.");

            socketAsyncEventArgs.AcceptSocket = null;
            _sendSocketOperations.Enqueue(socketAsyncEventArgs);
        }
    }
}