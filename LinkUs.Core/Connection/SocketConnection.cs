using System;
using System.Linq;
using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public class SocketConnection : IConnection
    {
        private readonly Socket _socket;
        private readonly SemaphoredQueue<SocketAsyncEventArgs> _receiveSocketOperations = new SemaphoredQueue<SocketAsyncEventArgs>();
        private readonly SemaphoredQueue<SocketAsyncEventArgs> _sendSocketOperations = new SemaphoredQueue<SocketAsyncEventArgs>();

        public event Action<byte[]> DataReceived;
        public event Action<int> DataSent;
        public event Action Closed;

        // ----- Constructors
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

        // ----- Public methods
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

        // ----- Receive Operation
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

            var bytesTransferredCount = receiveSocketEventArgs.BytesTransferred;
            if (bytesTransferredCount == 0) {
                CloseSocket(receiveSocketEventArgs.AcceptSocket);
                RecycleReceiveArgs(receiveSocketEventArgs);
                return;
            }
            var bytesTransferred = receiveSocketEventArgs.Buffer.Take(bytesTransferredCount).ToArray();
            ProcessBytesTransferred(receiveSocketEventArgs, bytesTransferred);
        }
        private void ProcessBytesTransferred(SocketAsyncEventArgs receiveSocketEventArgs, byte[] bytesTransferred)
        {
            var metadata = (Metadata) receiveSocketEventArgs.UserToken;
            if (metadata.PackageLength == 0) {
                var remainingBytesCountForPackageLength = metadata.PackageLengthBytes.Length - metadata.PackageLengthReceivedBytesCount;
                if (bytesTransferred.Length >= remainingBytesCountForPackageLength) {
                    Buffer.BlockCopy(
                        bytesTransferred,
                        0,
                        metadata.PackageLengthBytes,
                        metadata.PackageLengthReceivedBytesCount,
                        remainingBytesCountForPackageLength);

                    metadata.PackageLength = BitConverter.ToInt32(metadata.PackageLengthBytes, 0);
                    if (metadata.PackageLength <= 0 || metadata.PackageLength > 100000) {
                        throw new Exception("Invalid length");
                    }
                    metadata.Buffers.Add(bytesTransferred.Skip(remainingBytesCountForPackageLength).ToArray());
                }
                else {
                    Buffer.BlockCopy(
                        bytesTransferred,
                        0,
                        metadata.PackageLengthBytes,
                        metadata.PackageLengthReceivedBytesCount,
                        bytesTransferred.Length);

                    metadata.PackageLengthReceivedBytesCount += bytesTransferred.Length;
                    StartReceiveData(receiveSocketEventArgs);
                    return;
                }
            }
            else {
                metadata.Buffers.Add(bytesTransferred);
            }

            var allBytesReceivedCount = metadata.Buffers.Select(x => x.Length).Sum(x => x);
            if (allBytesReceivedCount == metadata.PackageLength) {
                DataReceived?.Invoke(metadata.Buffers.SelectMany(x => x).ToArray());
                metadata.Reset();
                StartReceiveData(receiveSocketEventArgs);
            }
            else if (allBytesReceivedCount < metadata.PackageLength) {
                StartReceiveData(receiveSocketEventArgs);
            }
            else {
                var allData = metadata.Buffers.SelectMany(x => x).ToArray();
                var exactData = allData.Take(metadata.PackageLength).ToArray();
                DataReceived?.Invoke(exactData);
                metadata.Reset();

                var surplusData = allData.Skip(exactData.Length).ToArray();
                ProcessBytesTransferred(receiveSocketEventArgs, surplusData);
            }
        }
        private void RecycleReceiveArgs(SocketAsyncEventArgs receiveSocketEventArgs)
        {
            receiveSocketEventArgs.AcceptSocket = null;
            ((Metadata) receiveSocketEventArgs.UserToken).Reset();
            _receiveSocketOperations.Enqueue(receiveSocketEventArgs);
        }

        // ----- Send Operation
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
            DataSent?.Invoke(args.BytesTransferred);
            RecycleSendSocket(args);
        }
        private void RecycleSendSocket(SocketAsyncEventArgs args)
        {
            args.AcceptSocket = null;
            ((Metadata) args.UserToken).Reset();
            _sendSocketOperations.Enqueue(args);
        }

        // ----- Interal logic
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
                args.SetBuffer(new byte[1024], 0, 1024);
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