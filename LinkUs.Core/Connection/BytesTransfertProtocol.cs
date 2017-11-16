using System;

namespace LinkUs.Core.Connection
{
    public class BytesTransfertProtocol
    {
        private const int HEADER_LENGTH = 4;

        private int? _packageLength;
        private byte[] _packageLengthBytes = new byte[HEADER_LENGTH];
        private int _packageLengthReceivedBytesCount;
        private byte[] _receivedMessage;
        private int _receivedBytesCount;

        private byte[] _messageToSend;
        private int _dataToSendOffset;

        // ----- Public methods
        public void PrepareMessageToSend(byte[] data)
        {
            _messageToSend = new byte[_packageLengthBytes.Length + data.Length];
            _packageLengthBytes = BitConverter.GetBytes(data.Length);
            Buffer.BlockCopy(_packageLengthBytes, 0, _messageToSend, 0, _packageLengthBytes.Length);
            Buffer.BlockCopy(data, 0, _messageToSend, _packageLengthBytes.Length, data.Length);
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

        public bool TryParse(ByteArraySlice dataSlice, out ParsedData parsedData)
        {
            var usedToParseHeaderBytesCount = ParseHeader(dataSlice);
            if (usedToParseHeaderBytesCount == dataSlice.Length) {
                parsedData = ParsedData.None();
                return false;
            }

            byte[] message;

            var messageSlice = dataSlice.ReduceSizeFromLeft(usedToParseHeaderBytesCount);

            var usedToParseMessageBytesCount = ParseMessage(
                messageSlice,
                out message
            );

            var remainingBytesCount = dataSlice.Length - usedToParseHeaderBytesCount - usedToParseMessageBytesCount;
            if (remainingBytesCount < 0) {
                throw new Exception("Cannot have a number of byte to process < 0.");
            }
            else if (remainingBytesCount == 0) {
                if (message == null) {
                    parsedData = ParsedData.None();
                    return false;
                }
                else {
                    parsedData = ParsedData.OnlyMessage(message);
                    return true;
                }
            }
            else /*if (remainingBytesCountToProcess > 0)*/ {
                parsedData = ParsedData.MessageAndAdditionalData(
                    message: message,
                    additionalData: dataSlice.ReduceSizeFromLeft(usedToParseHeaderBytesCount + usedToParseMessageBytesCount)
                );
                return true;
            }
        }
        public void Reset()
        {
            _packageLength = null;
            _packageLengthReceivedBytesCount = 0;
            _receivedBytesCount = 0;
            _receivedMessage = null;

            _dataToSendOffset = 0;
            _messageToSend = null;
        }

        // ----- Internal logic
        private int ParseHeader(ByteArraySlice byteArraySlice)
        {
            if (_packageLength.HasValue) {
                return 0;
            }

            var remainingBytesCountForPackageLength = _packageLengthBytes.Length - _packageLengthReceivedBytesCount;
            if (byteArraySlice.Length >= remainingBytesCountForPackageLength) {
                Buffer.BlockCopy(
                    byteArraySlice.Buffer,
                    0,
                    _packageLengthBytes,
                    _packageLengthReceivedBytesCount,
                    remainingBytesCountForPackageLength);

                _packageLength = BitConverter.ToInt32(_packageLengthBytes, 0);
                if (_packageLength <= 0 || _packageLength > 100000) {
                    throw new Exception("Invalid length");
                }
                _receivedMessage = new byte[_packageLength.Value];
                return remainingBytesCountForPackageLength;
            }
            else {
                Buffer.BlockCopy(
                    byteArraySlice.Buffer,
                    0,
                    _packageLengthBytes,
                    _packageLengthReceivedBytesCount,
                    byteArraySlice.Length);

                _packageLengthReceivedBytesCount += byteArraySlice.Length;
                return byteArraySlice.Length;
            }
        }
        private int ParseMessage(ByteArraySlice byteArraySlice, out byte[] message)
        {
            if (_packageLength.HasValue == false) {
                throw new Exception("Unable to process bytes received: the package length was not parsed.");
            }

            if (_receivedBytesCount + byteArraySlice.Length == _packageLength) {
                Buffer.BlockCopy(byteArraySlice.Buffer, byteArraySlice.Offset, _receivedMessage, _receivedBytesCount, byteArraySlice.Length);
                message = _receivedMessage;
                _receivedBytesCount += byteArraySlice.Length;
                return byteArraySlice.Length;
            }
            if (_receivedBytesCount + byteArraySlice.Length < _packageLength) {
                Buffer.BlockCopy(byteArraySlice.Buffer, byteArraySlice.Offset, _receivedMessage, _receivedBytesCount, byteArraySlice.Length);
                _receivedBytesCount += byteArraySlice.Length;
                message = null;
                return byteArraySlice.Length;
            }
            else {
                var remainingByteCount = _packageLength.Value - _receivedBytesCount;
                Buffer.BlockCopy(byteArraySlice.Buffer, byteArraySlice.Offset, _receivedMessage, _receivedBytesCount, remainingByteCount);
                _receivedBytesCount += remainingByteCount;
                message = _receivedMessage;
                return remainingByteCount;
            }
        }
    }
}