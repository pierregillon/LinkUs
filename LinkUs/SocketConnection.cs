using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace LinkUs
{
    public class SocketConnection : IConnection
    {
        private readonly Socket _socket;
        private readonly Queue<SocketAsyncEventArgs> _receiveSocketOperations = new Queue<SocketAsyncEventArgs>();
        private readonly Queue<SocketAsyncEventArgs> _sendSocketOperations = new Queue<SocketAsyncEventArgs>();

        public event Action<byte[]> DataReceived;
        public event Action<int> DataSent;
        public event Action Closed;

        public SocketConnection(Socket socket)
        {
            _socket = socket;

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

        public void StartContinuousReceive()
        {
            var receiveSocketEventArgs = _receiveSocketOperations.Dequeue();
            receiveSocketEventArgs.AcceptSocket = _socket;
            StartReceiveData(receiveSocketEventArgs);
        }
        public void Close()
        {
            CleanSocket(_socket);
        }
        public void SendAsync(byte[] data)
        {
            var sendSocketEventArgs = _sendSocketOperations.Dequeue();
            var metadata = (Metadata) sendSocketEventArgs.UserToken;
            var fullData = new byte[metadata.PackageLengthBytes.Length + data.Length];
            metadata.PackageLengthBytes = BitConverter.GetBytes(data.Length);
            Buffer.BlockCopy(metadata.PackageLengthBytes, 0, fullData, 0, metadata.PackageLengthBytes.Length);
            Buffer.BlockCopy(data, 0, fullData, metadata.PackageLengthBytes.Length, data.Length);

            sendSocketEventArgs.AcceptSocket = _socket;
            sendSocketEventArgs.SetBuffer(fullData, 0, fullData.Length);
            StartSendData(sendSocketEventArgs);
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
                ((Metadata) receiveSocketEventArgs.UserToken).Reset();
                _receiveSocketOperations.Enqueue(receiveSocketEventArgs);
                Closed?.Invoke();
                throw new Exception("bad operation");
            }
            if (receiveSocketEventArgs.SocketError == SocketError.ConnectionReset) {
                CleanSocket(receiveSocketEventArgs.AcceptSocket);
                receiveSocketEventArgs.AcceptSocket = null;
                ((Metadata) receiveSocketEventArgs.UserToken).Reset();
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
                DataReceived?.Invoke(bytesTransferred.Skip(metadata.PackageLengthBytes.Length).ToArray());
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
                ((Metadata) socketAsyncEventArgs.UserToken).Reset();
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
        }
    }
}