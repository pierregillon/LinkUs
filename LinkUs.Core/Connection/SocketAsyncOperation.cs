using System;
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
            Protocol.Reset();
            Protocol.PrepareMessageToSend(data);
            BufferInfo bufferInfo;
            if (Protocol.TryGetNextDataToSend(BUFFER_SIZE, out bufferInfo) == false) {
                throw new Exception("Unable to prepare the send operation: no data to send!");
            }
            SetBuffer(bufferInfo);
        }
        public void Reset()
        {
            Protocol.Reset();
        }
        public void PrepareReceiveOperation()
        {
            Protocol.Reset();
        }
        public bool PrepareNextSendOperation()
        {
            BufferInfo bufferInfo;

            if (!Protocol.TryGetNextDataToSend(BUFFER_SIZE, out bufferInfo)) {
                return false;
            }

            SetBuffer(bufferInfo);

            return true;
        }

        // ----- Internal logic
        private void SetBuffer(BufferInfo bufferInfo)
        {
            System.Buffer.BlockCopy(
                bufferInfo.Buffer,
                bufferInfo.Offset,
                Buffer,
                0,
                bufferInfo.Length);

            SetBuffer(0, bufferInfo.Length);
        }
    }
}