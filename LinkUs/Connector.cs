using System;
using System.Collections.Generic;
using System.Data;
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
        public event Action<ClientId> ClientDisconnected;
        protected virtual void OnClientDisconnected(ClientId clientId)
        {
            ClientDisconnected?.Invoke(clientId);
        }
        public event Action<Package> PackageReceived;
        protected virtual void OnPackageReceived(Package obj)
        {
            PackageReceived?.Invoke(obj);
        }

        // ----- Constructor

        public Connector()
        {
            for (int i = 0; i < 5; i++) {
                var args = new SocketAsyncEventArgs();
                args.Completed += AcceptEventCompleted;
                _acceptSocketOperations.Enqueue(args);
            }

            for (int i = 0; i < 10; i++) {
                var args = new SocketAsyncEventArgs();
                args.Completed += ReceiveEventCompleted;
                args.SetBuffer(new byte[10000], 0, 10000);
                args.UserToken = new Metadata();
                _receiveSocketOperations.Enqueue(args);
            }

            for (int i = 0; i < 10; i++) {
                var args = new SocketAsyncEventArgs();
                args.Completed += SendEventCompleted;
                args.UserToken = new Metadata();
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
            var data = package.ToByteArray();
            var socket = _connectedSockets[package.Destination];
            var sendSocketEventArgs = _sendSocketOperations.Dequeue();
            var metadata = (Metadata) sendSocketEventArgs.UserToken;
            var fullData = new byte[metadata.PackageLengthBytes.Length + data.Length];
            metadata.PackageLengthBytes = BitConverter.GetBytes(data.Length);
            Buffer.BlockCopy(metadata.PackageLengthBytes, 0, fullData, 0, metadata.PackageLengthBytes.Length);
            Buffer.BlockCopy(data, 0, fullData, metadata.PackageLengthBytes.Length, data.Length);

            sendSocketEventArgs.AcceptSocket = socket;
            sendSocketEventArgs.SetBuffer(fullData, 0, fullData.Length);
            StartSendData(sendSocketEventArgs);
        }
        public void Close()
        {
            try {
                _listenSocket.Shutdown(SocketShutdown.Both);
            }
            // Stopping asynchrone operation can throw exception.
            catch (SocketException) { }

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

            var receiveSocketEventArgs = _receiveSocketOperations.Dequeue();
            receiveSocketEventArgs.AcceptSocket = acceptSocketEventArgs.AcceptSocket;
            ((Metadata)receiveSocketEventArgs.UserToken).ClientId = clientId;
            StartReceiveData(receiveSocketEventArgs);

            acceptSocketEventArgs.AcceptSocket = null;
            _acceptSocketOperations.Enqueue(acceptSocketEventArgs);
        }

        private void StartReceiveData(SocketAsyncEventArgs args)
        {
            var isPending = args.AcceptSocket.ReceiveAsync(args);
            if (!isPending) {
                ProcessReceiveData(args);
            }
        }
        private void ReceiveEventCompleted(object sender, SocketAsyncEventArgs receiveSocketEventArgs)
        {
            ProcessReceiveData(receiveSocketEventArgs);
        }
        private void ProcessReceiveData(SocketAsyncEventArgs receiveSocketEventArgs)
        {
            if (receiveSocketEventArgs.LastOperation != SocketAsyncOperation.Receive) {
                CleanSocket(receiveSocketEventArgs.AcceptSocket);
                receiveSocketEventArgs.AcceptSocket = null;
                ((Metadata)receiveSocketEventArgs.UserToken).Reset();
                _receiveSocketOperations.Enqueue(receiveSocketEventArgs);
                throw new Exception("bad operation");
            }
            if (receiveSocketEventArgs.SocketError == SocketError.ConnectionReset) {
                CleanSocket(receiveSocketEventArgs.AcceptSocket);
                receiveSocketEventArgs.AcceptSocket = null;
                ((Metadata)receiveSocketEventArgs.UserToken).Reset();
                _receiveSocketOperations.Enqueue(receiveSocketEventArgs);
                return;
            }
            if (receiveSocketEventArgs.SocketError != SocketError.Success) {
                throw new Exception("unsuccessed read");
            }

            var metadata = (Metadata) receiveSocketEventArgs.UserToken;

            var bytesTransferredCount = receiveSocketEventArgs.BytesTransferred;
            var bytesTransferred = receiveSocketEventArgs.Buffer.Take(bytesTransferredCount).ToArray();
            if (metadata.PackageLength == 0) {
                Buffer.BlockCopy(bytesTransferred, 0, metadata.PackageLengthBytes, 0, metadata.PackageLengthBytes.Length);
                metadata.PackageLength = BitConverter.ToInt32(metadata.PackageLengthBytes, 0);
            }
            if (bytesTransferredCount - metadata.PackageLengthBytes.Length == metadata.PackageLength) {
                var package = Package.Parse(bytesTransferred.Skip(metadata.PackageLengthBytes.Length).ToArray());
                package.ChangeSource(metadata.ClientId);
                OnPackageReceived(package);
            }
            else {
                throw new NotImplementedException();
            }

            metadata.PackageLength = 0;
            StartReceiveData(receiveSocketEventArgs);
        }

        private void StartSendData(SocketAsyncEventArgs args)
        {
            var isPending = args.AcceptSocket.SendAsync(args);
            if (isPending == false) {
                ProcessSendData(args);
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
                CleanSocket(socketAsyncEventArgs.AcceptSocket);
                socketAsyncEventArgs.AcceptSocket = null;
                ((Metadata)socketAsyncEventArgs.UserToken).Reset();
                _sendSocketOperations.Enqueue(socketAsyncEventArgs);
                throw new Exception("unsuccessed send");
            }

            var bytesTransferred = socketAsyncEventArgs.BytesTransferred;

            socketAsyncEventArgs.AcceptSocket = null;
            _sendSocketOperations.Enqueue(socketAsyncEventArgs);
        }

        private void CleanSocket(Socket socket)
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
            socket.Dispose();

            var entry = _connectedSockets.Single(x => x.Value == socket);
            _connectedSockets.Remove(entry.Key);
            OnClientDisconnected(entry.Key);
        }
    }

    public class Metadata
    {
        public ClientId ClientId;
        public List<byte[]> Buffers;
        public byte[] PackageLengthBytes = new byte[4];
        public int PackageLength = 0;

        public void Reset()
        {
            ClientId = null;
            Buffers.Clear();
            PackageLength = 0;
        }
    }
}