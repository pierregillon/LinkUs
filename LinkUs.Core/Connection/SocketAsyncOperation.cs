using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public class SocketAsyncOperation : SocketAsyncEventArgs
    {
        private const int BUFFER_SIZE = 1024;

        public BytesTransfertProtocol Protocol { get; } = new BytesTransfertProtocol();

        // ----- Constructors
        public SocketAsyncOperation()
        {
            SetBuffer(new byte[BUFFER_SIZE], 0, BUFFER_SIZE);
        }

        // ----- Public methods
        public void PrepareSendOperation(byte[] data)
        {
            var dataToSend = Protocol.PrepareMessageToSend(data);
            SetBuffer(dataToSend, 0, dataToSend.Length);
        }
        public void Reset()
        {
            AcceptSocket = null;
            Protocol.Reset();
        }
        public void PrepareReceiveOperation()
        {
            Protocol.Reset();
        }
    }
}