using System;

namespace LinkUs.Core.Connection
{
    public class ReadBytesTransfertProtocol
    {
        private const int HEADER_LENGTH = 4;

        private readonly byte[] _packageLengthBytes = new byte[HEADER_LENGTH];
        private int _packageLengthReceivedBytesCount;
        private byte[] _receivedMessage;
        private int _receivedBytesCount;

        // ----- Public methods
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
            _packageLengthReceivedBytesCount = 0;
            _receivedBytesCount = 0;
            _receivedMessage = null;
        }

        // ----- Internal logic
        private int ParseHeader(ByteArraySlice byteArraySlice)
        {
            if (_receivedMessage != null) {
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

                var messageLength = BitConverter.ToInt32(_packageLengthBytes, 0);
                if (messageLength <= 0 || messageLength > 100000) {
                    throw new Exception("Invalid length");
                }
                _receivedMessage = new byte[messageLength];
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
            if (_receivedMessage == null) {
                throw new Exception("Unable to process bytes received: the received message was not instanciated.");
            }

            if (_receivedBytesCount + byteArraySlice.Length == _receivedMessage.Length) {
                byteArraySlice.CopyTo(_receivedMessage, _receivedBytesCount);
                message = _receivedMessage;
                _receivedBytesCount += byteArraySlice.Length;
                return byteArraySlice.Length;
            }
            if (_receivedBytesCount + byteArraySlice.Length < _receivedMessage.Length) {
                byteArraySlice.CopyTo(_receivedMessage, _receivedBytesCount);
                _receivedBytesCount += byteArraySlice.Length;
                message = null;
                return byteArraySlice.Length;
            }
            else {
                var remainingByteCount = _receivedMessage.Length - _receivedBytesCount;
                byteArraySlice.CopyTo(_receivedMessage, _receivedBytesCount, remainingByteCount);
                _receivedBytesCount += remainingByteCount;
                message = _receivedMessage;
                return remainingByteCount;
            }
        }
    }
}