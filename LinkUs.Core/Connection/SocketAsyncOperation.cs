using System;
using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public class SocketAsyncOperation : SocketAsyncEventArgs
    {
        private const int BUFFER_SIZE = 1024;

        private readonly ByteArraySliceAggregator _sliceAggregator = new ByteArraySliceAggregator();
        private readonly ByteArraySlicer _byteArraySlicer = new ByteArraySlicer();

        // ----- Constructors
        public SocketAsyncOperation()
        {
            SetBuffer(new byte[BUFFER_SIZE], 0, BUFFER_SIZE);
        }

        // ----- Public methods
        public void PrepareSendOperation(Socket socket, byte[] data)
        {
            AcceptSocket = socket;
            _byteArraySlicer.DefineMessageToSlice(data);
            ByteArraySlice byteArraySlice;
            if (_byteArraySlicer.TryGetNextSlice(BUFFER_SIZE, out byteArraySlice) == false) {
                throw new Exception("Unable to prepare the send operation: no data to send!");
            }
            SetBuffer(byteArraySlice);
        }
        public bool PrepareNextSendOperation(int byteTransferred)
        {
            _byteArraySlicer.AcquitBytes(byteTransferred);
            ByteArraySlice byteArraySlice;
            if (!_byteArraySlicer.TryGetNextSlice(BUFFER_SIZE, out byteArraySlice)) {
                return false;
            }
            SetBuffer(byteArraySlice);
            return true;
        }
        public ByteArraySlice DigestSliceReceived(ByteArraySlice slice, Action<byte[]> dataReceived)
        {
            _sliceAggregator.Aggregate(slice);

            if (!_sliceAggregator.IsFinished()) {
                return null;
            }

            var message = _sliceAggregator.GetBuiltMessage();
            dataReceived?.Invoke(message);
            var additionalData = _sliceAggregator.GetAdditionalData();
            _sliceAggregator.Reset();
            return additionalData;
        }
        public void Clean()
        {
            _sliceAggregator.Reset();
            _byteArraySlicer.Reset();
        }

        // ----- Internal logic
        private void SetBuffer(ByteArraySlice slice)
        {
            slice.CopyTo(Buffer);
            SetBuffer(0, slice.Length);
        }
        public void PrepareReceiveOperation(Socket socket)
        {
            AcceptSocket = socket;
        }
    }
}