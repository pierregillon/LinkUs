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
            ByteArraySlice byteArraySlice;
            if (Protocol.TryGetNextDataToSend(BUFFER_SIZE, out byteArraySlice) == false) {
                throw new Exception("Unable to prepare the send operation: no data to send!");
            }
            SetBuffer(byteArraySlice);
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
            ByteArraySlice byteArraySlice;

            if (!Protocol.TryGetNextDataToSend(BUFFER_SIZE, out byteArraySlice)) {
                return false;
            }

            SetBuffer(byteArraySlice);

            return true;
        }

        // ----- Internal logic
        private void SetBuffer(ByteArraySlice byteArraySlice)
        {
            System.Buffer.BlockCopy(
                byteArraySlice.Buffer,
                byteArraySlice.Offset,
                Buffer,
                0,
                byteArraySlice.Length);

            SetBuffer(0, byteArraySlice.Length);
        }
    }
}