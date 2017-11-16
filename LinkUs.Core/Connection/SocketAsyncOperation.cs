using System;
using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public class SocketAsyncOperation : SocketAsyncEventArgs
    {
        private const int BUFFER_SIZE = 1024;

        public ByteArraySliceAggregator ByteArraySliceAggregator { get; } = new ByteArraySliceAggregator();
        public ByteArraySlicer ByteArraySlicer { get; } = new ByteArraySlicer();

        // ----- Constructors
        public SocketAsyncOperation()
        {
            SetBuffer(new byte[BUFFER_SIZE], 0, BUFFER_SIZE);
        }

        // ----- Public methods
        public void PrepareReceiveOperation()
        {
            ByteArraySliceAggregator.Reset();
        }
        public void PrepareSendOperation(byte[] data)
        {
            ByteArraySlicer.Reset();
            ByteArraySlicer.DefineMessageToSlice(data);
            ByteArraySlice byteArraySlice;
            if (ByteArraySlicer.TryGetNextSlice(BUFFER_SIZE, out byteArraySlice) == false) {
                throw new Exception("Unable to prepare the send operation: no data to send!");
            }
            SetBuffer(byteArraySlice);
        }
        public bool PrepareNextSendOperation(int byteTransferred)
        {
            ByteArraySlicer.AcquitBytes(byteTransferred);

            ByteArraySlice byteArraySlice;

            if (!ByteArraySlicer.TryGetNextSlice(BUFFER_SIZE, out byteArraySlice)) {
                return false;
            }

            SetBuffer(byteArraySlice);

            return true;
        }
        public void Clean()
        {
            ByteArraySliceAggregator.Reset();
            ByteArraySlicer.Reset();
        }

        // ----- Internal logic
        private void SetBuffer(ByteArraySlice slice)
        {
            slice.CopyTo(Buffer);
            SetBuffer(0, slice.Length);
        }
    }
}