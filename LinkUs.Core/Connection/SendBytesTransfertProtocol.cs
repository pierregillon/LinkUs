using System;

namespace LinkUs.Core.Connection
{
    public class SendBytesTransfertProtocol
    {
        private const int HEADER_LENGTH = 4;

        private byte[] _messageToSend;
        private int _dataToSendOffset;

        // ----- Public methods
        public void PrepareMessageToSend(byte[] data)
        {
            _messageToSend = new byte[HEADER_LENGTH + data.Length];
            var messageLength = BitConverter.GetBytes(data.Length);
            Buffer.BlockCopy(messageLength, 0, _messageToSend, 0, messageLength.Length);
            Buffer.BlockCopy(data, 0, _messageToSend, messageLength.Length, data.Length);
        }
        public bool TryGetNextDataToSend(int dataSize, out ByteArraySlice slice)
        {
            if (_messageToSend == null) {
                throw new Exception("Unable to get next data to send, no message prepared.");
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
        public void AcquitSentBytes(int byteSentCount)
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