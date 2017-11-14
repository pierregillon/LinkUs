using System;
using System.Linq;
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
            BuildSocketAsyncOperation();
        }
        public SocketConnection(Socket socket)
        {
            _socket = socket;
            BuildSocketAsyncOperation();
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
            var protocol = operation.Protocol;
            var dataToSend = protocol.Transform(data);
            operation.AcceptSocket = _socket;
            operation.SetBuffer(dataToSend, 0, dataToSend.Length);
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
            var bytesTransferred = operation.Buffer.Take(bytesTransferredCount).ToArray();
            ProcessBytesTransferred(operation, bytesTransferred);
        }
        private void ProcessBytesTransferred(SocketAsyncOperation operation, byte[] bytesTransferred)
        {
            var protocol = operation.Protocol;
            var usedToProcessHeaderBytesCount = protocol.ProcessHeader(bytesTransferred);
            if (usedToProcessHeaderBytesCount == bytesTransferred.Length) {
                StartReceiveOperationAsync(operation);
                return;
            }

            var messageBytes = ReduceBuffer(bytesTransferred, usedToProcessHeaderBytesCount);
            var usedToProcessMessageBytesCount = protocol.ProcessMessage(messageBytes, DataReceived);

            var remainingBytesCountToProcess = bytesTransferred.Length - usedToProcessHeaderBytesCount - usedToProcessMessageBytesCount;
            if (remainingBytesCountToProcess == 0) {
                StartReceiveOperationAsync(operation);
            }
            else if (remainingBytesCountToProcess > 0) {
                var surplusData = bytesTransferred.Skip(usedToProcessHeaderBytesCount + usedToProcessMessageBytesCount).ToArray();
                ProcessBytesTransferred(operation, surplusData);
            }
            else {
                throw new Exception("Cannot have a number of byte to process < 0.");
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
            RecycleOperation(operation);
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
            operation.AcceptSocket = _socket;
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
        private void BuildSocketAsyncOperation()
        {
            for (var i = 0; i < ASYNC_OPERATION_COUNT; i++) {
                var operation = new SocketAsyncOperation();
                operation.Completed += EventCompleted;
                _socketOperations.Enqueue(operation);
            }
        }
        private static Socket BuildDefaultSocket()
        {
            return new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }
        private static byte[] ReduceBuffer(byte[] bytesTransferred, int usedToProcessHeaderBytesCount)
        {
            byte[] buffer;
            if (usedToProcessHeaderBytesCount == 0) {
                buffer = bytesTransferred;
            }
            else {
                var dataToProcess = bytesTransferred.Skip(usedToProcessHeaderBytesCount).ToArray();
                buffer = dataToProcess;
            }
            return buffer;
        }
    }

    public class SocketAsyncOperation : SocketAsyncEventArgs
    {
        public BytesTransfertProtocol Protocol { get; } = new BytesTransfertProtocol();

        public SocketAsyncOperation()
        {
            SetBuffer(new byte[1024], 0, 1024);
        }

        public void Reset()
        {
            AcceptSocket = null;
            Protocol.Reset();
        }
    }
}