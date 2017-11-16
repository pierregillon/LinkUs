using System;
using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public class SocketConnection : IConnection
    {
        private const int ASYNC_OPERATION_COUNT = 20;

        private readonly Socket _socket;
        private readonly SemaphoredQueue<SocketAsyncOperation> _socketOperations = new SemaphoredQueue<SocketAsyncOperation>(ASYNC_OPERATION_COUNT);

        public event Action<byte[]> DataReceived;
        public event Action<int> DataSent;
        public event Action Closed;

        // ----- Constructors
        public SocketConnection()
        {
            _socket = BuildDefaultSocket();
            BuildSocketAsyncOperation(_socket);
        }
        public SocketConnection(Socket socket)
        {
            _socket = socket;
            BuildSocketAsyncOperation(socket);
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
            var operation = _socketOperations.Dequeue();
            operation.PrepareSendOperation(data);
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
            ProcessBytesTransferred(operation, operation.Buffer, bytesTransferredCount);
        }
        private void ProcessBytesTransferred(SocketAsyncOperation operation, byte[] bytesTransferred, int bytesTransferredCount)
        {
            var protocol = operation.Protocol;

            ParsedData parsedData;
            var extractionSucceded = protocol.TryParse(bytesTransferred, bytesTransferredCount, out parsedData);
            if (!extractionSucceded) {
                StartReceiveOperationAsync(operation);
            }
            else {
                DataReceived?.Invoke(parsedData.Message);
                operation.PrepareReceiveOperation();
                if (parsedData.ContainsAdditionalData()) {
                    ProcessBytesTransferred(operation, parsedData.AdditionalData, parsedData.AdditionalData.Length);
                }
                else {
                    StartReceiveOperationAsync(operation);
                }
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

            operation.Protocol.AcquitSentBytes(operation.BytesTransferred);

            if (operation.PrepareNextSendOperation()) {
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
            operation.Reset();
            _socketOperations.Enqueue(operation);
        }
        private void StartContinuousReceive()
        {
            var operation = _socketOperations.Dequeue();
            operation.PrepareReceiveOperation();
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
        private void BuildSocketAsyncOperation(Socket socket)
        {
            for (var i = 0; i < ASYNC_OPERATION_COUNT; i++) {
                var operation = new SocketAsyncOperation();
                operation.Completed += EventCompleted;
                operation.AcceptSocket = socket;
                _socketOperations.Enqueue(operation);
            }
        }
        private static Socket BuildDefaultSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
    }
}