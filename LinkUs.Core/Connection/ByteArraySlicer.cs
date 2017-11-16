using System;

namespace LinkUs.Core.Connection
{
    public class ByteArraySlicer
    {
        private const int INTEGER_SIZE = 4;

        private byte[] _messageToSend;
        private int _dataToSendOffset;

        // ----- Public methods
        public void DefineMessageToSlice(byte[] data)
        {
            _messageToSend = new byte[INTEGER_SIZE + data.Length];
            var messageLength = BitConverter.GetBytes(data.Length);
            Buffer.BlockCopy(messageLength, 0, _messageToSend, 0, messageLength.Length);
            Buffer.BlockCopy(data, 0, _messageToSend, messageLength.Length, data.Length);
        }
        public bool TryGetNextSlice(int dataSize, out ByteArraySlice slice)
        {
            if (_messageToSend == null) {
                throw new Exception("Unable to get next data slice, no message defined.");
            }
            var remainingBytesToSendCount = _messageToSend.Length - _dataToSendOffset;
            if (remainingBytesToSendCount == 0) {
                slice = null;
                return false;
            }
            else {
                slice = new ByteArraySlice(
                    _messageToSend,
                    Math.Min(dataSize, remainingBytesToSendCount),
                    _dataToSendOffset
                );
                return true;
            }
        }
        public void AcquitBytes(int byteSentCount)
        {
            if (byteSentCount + _dataToSendOffset > _messageToSend.Length) {
                throw new Exception("Cannot ACK more bytes than the message contains.");
            }

            _dataToSendOffset += byteSentCount;
        }
        public void Reset()
        {
            _dataToSendOffset = 0;
            _messageToSend = null;
        }
    }
}