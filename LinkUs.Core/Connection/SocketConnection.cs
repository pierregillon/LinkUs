using System;
using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public class SocketConnection : IConnection
    {
        private readonly SocketAsyncOperationPool _operationPool;
        private readonly Socket _socket;
        
        public event Action<byte[]> DataReceived;
        public event Action<int> DataSent;
        public event Action Closed;

        // ----- Constructors
        public SocketConnection(SocketAsyncOperationPool operationPool)
        {
            _operationPool = operationPool;
            _socket = BuildDefaultSocket();
        }
        public SocketConnection(SocketAsyncOperationPool operationPool, Socket socket)
        {
            _operationPool = operationPool;
            _socket = socket;
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
            var operation = _operationPool.Dequeue();
            operation.PrepareSendOperation(_socket, data);
            operation.Completed += EventCompleted;
            StartSendOperationAsync(operation);
        }
        public void Close()
        {
            CloseSocket(_socket);
        }

        // ----- Receive Operation
        private void StartReceiveOperationAsync(SocketAsyncOperation operation)
        {
            var isPending = operation.AcceptSocket.ReceiveAsync(operation);
            if (!isPending) {
                EndReceiveOperation(operation);
            }
        }
        private void EndReceiveOperation(SocketAsyncOperation operation)
        {
            if (operation.SocketError == SocketError.ConnectionReset) {
                CloseSocket(operation.AcceptSocket);
                RecycleOperation(operation);
                return;
            }

            if (operation.SocketError != SocketError.Success) {
                CloseSocket(operation.AcceptSocket);
                RecycleOperation(operation);
                throw new Exception("Read failed");
            }

            var bytesTransferredCount = operation.BytesTransferred;
            if (bytesTransferredCount == 0) {
                CloseSocket(operation.AcceptSocket);
                RecycleOperation(operation);
                return;
            }

            var dataSlice = new ByteArraySlice(operation.Buffer, bytesTransferredCount);
            ProcessBytesTransferred(operation, dataSlice);
        }
        private void ProcessBytesTransferred(SocketAsyncOperation operation, ByteArraySlice byteArraySliceRead)
        {
            var additionalData = operation.DigestSliceReceived(byteArraySliceRead, DataReceived);
            if (additionalData != null) {
                ProcessBytesTransferred(operation, additionalData);
            }
            else {
                StartReceiveOperationAsync(operation);
            }
        }

        // ----- Send Operation
        private void StartSendOperationAsync(SocketAsyncOperation operation)
        {
            try {
                var isPending = operation.AcceptSocket.SendAsync(operation);
                if (isPending == false) {
                    EndSendOperation(operation);
                }
            }
            catch (ObjectDisposedException ex) {
                if (ex.ObjectName == "System.Net.Sockets.Socket") {
                    CloseSocket(operation.AcceptSocket);
                    RecycleOperation(operation);
                    return;
                }
                throw;
            }
        }
        private void EndSendOperation(SocketAsyncOperation operation)
        {
            if (operation.SocketError == SocketError.ConnectionReset) {
                CloseSocket(operation.AcceptSocket);
                RecycleOperation(operation);
                return;
            }
            if (operation.SocketError != SocketError.Success) {
                CloseSocket(operation.AcceptSocket);
                RecycleOperation(operation);
                throw new Exception("unsuccessed send");
            }

            DataSent?.Invoke(operation.BytesTransferred);

            if (operation.PrepareNextSendOperation(operation.BytesTransferred)) {
                StartSendOperationAsync(operation);
            }
            else {
                RecycleOperation(operation);
            }
        }

        // ----- Callbacks
        private void EventCompleted(object sender, SocketAsyncEventArgs operation)
        {
            if (operation.LastOperation == System.Net.Sockets.SocketAsyncOperation.Send) {
                EndSendOperation((SocketAsyncOperation) operation);
            }
            else if (operation.LastOperation == System.Net.Sockets.SocketAsyncOperation.Receive) {
                EndReceiveOperation((SocketAsyncOperation) operation);
            }
            else {
                throw new NotImplementedException("Unknown of SocketAsyncOperation");
            }
        }

        // ----- Interal logic
        private void RecycleOperation(SocketAsyncOperation operation)
        {
            operation.Clean();
            operation.Completed -= EventCompleted;
            _operationPool.Enqueue(operation);
        }
        private void StartContinuousReceive()
        {
            var operation = _operationPool.Dequeue();
            operation.PrepareReceiveOperation(_socket);
            operation.Completed += EventCompleted;
            StartReceiveOperationAsync(operation);
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
        private static Socket BuildDefaultSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}