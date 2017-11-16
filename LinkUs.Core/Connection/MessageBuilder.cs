using System;

namespace LinkUs.Core.Connection
{
    public class MessageBuilder
    {
        private const int INTEGER_SIZE = 4;

        private readonly byte[] _messageLengthBytes = new byte[INTEGER_SIZE];
        private byte[] _builtMessage;
        private int _messageLengthBytesReceivedCount;
        private int _messageBytesReceivedCount;
        private ByteArraySlice _additionalData;

        // ----- Public methods
        public void AddData(ByteArraySlice dataSlice)
        {
            var usedToParseHeaderBytesCount = ParseMessageLength(dataSlice);
            if (usedToParseHeaderBytesCount == dataSlice.Length) {
                return;
            }

            var messageSlice = dataSlice.ReduceSizeFromLeft(usedToParseHeaderBytesCount);
            var usedToParseMessageBytesCount = ParseMessageContent(messageSlice);

            var remainingBytesCount = dataSlice.Length - usedToParseHeaderBytesCount - usedToParseMessageBytesCount;
            if (remainingBytesCount < 0) {
                throw new Exception("Cannot have a number of byte to process < 0.");
            }
            if (remainingBytesCount != 0) {
                _additionalData = dataSlice.ReduceSizeFromLeft(usedToParseHeaderBytesCount + usedToParseMessageBytesCount);
            }
        }
        public bool IsFinished()
        {
            return _messageBytesReceivedCount == _builtMessage?.Length;
        }
        public byte[] GetBuiltMessage()
        {
            return _builtMessage;
        }
        public ByteArraySlice GetAdditionalData()
        {
            return _additionalData;
        }
        public void Reset()
        {
            _messageLengthBytesReceivedCount = 0;
            _messageBytesReceivedCount = 0;
            _builtMessage = null;
            _additionalData = null;
        }

        // ----- Internal logic
        private int ParseMessageLength(ByteArraySlice data)
        {
            if (_builtMessage != null) {
                return 0;
            }

            var remainingBytesCountForMessageLength = _messageLengthBytes.Length - _messageLengthBytesReceivedCount;
            if (data.Length >= remainingBytesCountForMessageLength) {
                data.CopyTo(_messageLengthBytes,
                            _messageLengthBytesReceivedCount,
                            remainingBytesCountForMessageLength);
                _builtMessage = BuildEmptyMessage(_messageLengthBytes);
                return remainingBytesCountForMessageLength;
            }
            else {
                data.CopyTo(_messageLengthBytes, _messageLengthBytesReceivedCount);
                _messageLengthBytesReceivedCount += data.Length;
                return data.Length;
            }
        }
        private int ParseMessageContent(ByteArraySlice byteArraySlice)
        {
            if (_builtMessage == null) {
                throw new Exception("Unable to process bytes received: the received message was not instanciated.");
            }

            if (_messageBytesReceivedCount + byteArraySlice.Length == _builtMessage.Length) {
                byteArraySlice.CopyTo(_builtMessage, _messageBytesReceivedCount);
                _messageBytesReceivedCount += byteArraySlice.Length;
                return byteArraySlice.Length;
            }
            if (_messageBytesReceivedCount + byteArraySlice.Length < _builtMessage.Length) {
                byteArraySlice.CopyTo(_builtMessage, _messageBytesReceivedCount);
                _messageBytesReceivedCount += byteArraySlice.Length;
                return byteArraySlice.Length;
            }
            else {
                var remainingByteCount = _builtMessage.Length - _messageBytesReceivedCount;
                byteArraySlice.CopyTo(_builtMessage, _messageBytesReceivedCount, remainingByteCount);
                _messageBytesReceivedCount += remainingByteCount;
                return remainingByteCount;
            }
        }

        // ----- Utils
        private static byte[] BuildEmptyMessage(byte[] messageLengthBytes)
        {
            var messageLength = BitConverter.ToInt32(messageLengthBytes, 0);
            if (messageLength <= 0 || messageLength > 100000) {
                throw new Exception("Invalid length");
            }
            return new byte[messageLength];
        }
    }
}