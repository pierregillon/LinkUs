using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace LinkUs.Core
{
    public class SocketConnection : IConnection
    {
        private readonly Socket _socket;
        private readonly Queue<SocketAsyncEventArgs> _receiveSocketOperations = new Queue<SocketAsyncEventArgs>();
        private readonly Queue<SocketAsyncEventArgs> _sendSocketOperations = new Queue<SocketAsyncEventArgs>();

        public event Action<byte[]> DataReceived;
        public event Action<int> DataSent;
        public event Action Closed;

        public SocketConnection()
        {
            _socket = BuildDefaultSocket();
            BuildSocketAsyncEventArgs();
        }
        public SocketConnection(Socket socket)
        {
            _socket = socket;
            BuildSocketAsyncEventArgs();
            StartContinuousReceive();
        }

        public void Connect(string host, int port)
        {
            _socket.Connect(host, port);
            StartContinuousReceive();
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
        public void Close()
        {
            CloseSocket(_socket);
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
                CloseSocket(receiveSocketEventArgs.AcceptSocket);
                RecycleReceiveArgs(receiveSocketEventArgs);
                throw new Exception("bad operation");
            }
            if (receiveSocketEventArgs.SocketError == SocketError.ConnectionReset) {
                CloseSocket(receiveSocketEventArgs.AcceptSocket);
                RecycleReceiveArgs(receiveSocketEventArgs);
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
        private void RecycleReceiveArgs(SocketAsyncEventArgs receiveSocketEventArgs)
        {
            receiveSocketEventArgs.AcceptSocket = null;
            ((Metadata) receiveSocketEventArgs.UserToken).Reset();
            _receiveSocketOperations.Enqueue(receiveSocketEventArgs);
        }

        private void StartSendData(SocketAsyncEventArgs args)
        {
            try {
                var isPending = args.AcceptSocket.SendAsync(args);
                if (isPending == false) {
                    ProcessSendData(args);
                }
            }
            catch (ObjectDisposedException ex) {
                if (ex.ObjectName == "System.Net.Sockets.Socket") {
                    CloseSocket(args.AcceptSocket);
                    RecycleSendSocket(args);
                    return;
                }
                throw;
            }
        }
        private void SendEventCompleted(object sender, SocketAsyncEventArgs asyncEventArgs)
        {
            ProcessSendData(asyncEventArgs);
        }
        private void ProcessSendData(SocketAsyncEventArgs args)
        {
            if (args.LastOperation != SocketAsyncOperation.Send) {
                CloseSocket(args.AcceptSocket);
                RecycleSendSocket(args);
                throw new Exception("bad operation");
            }
            if (args.SocketError != SocketError.Success) {
                CloseSocket(args.AcceptSocket);
                RecycleSendSocket(args);
                throw new Exception("unsuccessed send");
            }

            var bytesTransferred = args.BytesTransferred;

            RecycleSendSocket(args);
        }
        private void RecycleSendSocket(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            ((Metadata) args.UserToken).Reset();
            _sendSocketOperations.Enqueue(args);
        }

        private void StartContinuousReceive()
        {
            var receiveSocketEventArgs = _receiveSocketOperations.Dequeue();
            receiveSocketEventArgs.AcceptSocket = _socket;
            StartReceiveData(receiveSocketEventArgs);
        }
        private void CloseSocket(Socket socket)
        {
            try {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch (ObjectDisposedException) { }
            socket.Close();
            socket.Dispose();
            Closed?.Invoke();
        }

        private void BuildSocketAsyncEventArgs()
        {
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
        private static Socket BuildDefaultSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}