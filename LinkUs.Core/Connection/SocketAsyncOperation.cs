using System;
using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public class SocketAsyncOperation : SocketAsyncEventArgs
    {
        private const int BUFFER_SIZE = 1024;

        public ReadBytesTransfertProtocol ReadProtocol { get; } = new ReadBytesTransfertProtocol();
        public SendBytesTransfertProtocol SendProtocol { get; } = new SendBytesTransfertProtocol();

        // ----- Constructors
        public SocketAsyncOperation()
        {
            SetBuffer(new byte[BUFFER_SIZE], 0, BUFFER_SIZE);
        }

        // ----- Public methods
        public void PrepareReceiveOperation()
        {
            ReadProtocol.Reset();
        }
        public void PrepareSendOperation(byte[] data)
        {
            SendProtocol.Reset();
            SendProtocol.PrepareMessageToSend(data);
            ByteArraySlice byteArraySlice;
            if (SendProtocol.TryGetNextDataToSend(BUFFER_SIZE, out byteArraySlice) == false) {
                throw new Exception("Unable to prepare the send operation: no data to send!");
            }
            SetBuffer(byteArraySlice);
        }
        public bool PrepareNextSendOperation(int byteTransferred)
        {
            SendProtocol.AcquitSentBytes(byteTransferred);

            ByteArraySlice byteArraySlice;

            if (!SendProtocol.TryGetNextDataToSend(BUFFER_SIZE, out byteArraySlice)) {
                return false;
            }

            SetBuffer(byteArraySlice);

            return true;
        }
        public void Clean()
        {
            ReadProtocol.Reset();
            SendProtocol.Reset();
        }

        // ----- Internal logic
        private void SetBuffer(ByteArraySlice slice)
        {
            slice.CopyTo(Buffer);
            SetBuffer(0, slice.Length);
        }
    }
}