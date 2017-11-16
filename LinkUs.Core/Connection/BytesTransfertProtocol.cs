using System;
using System.Linq;

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
        public bool TryGetNextDataToSend(int dataSize, out BufferInfo bufferInfo)
        {
            if (_messageToSend == null) {
                throw new Exception("Unable to get next data to send, no message prepared.");
            }
            var remainingBytesToSendCount = _messageToSend.Length - _dataToSendOffset;
            if (remainingBytesToSendCount == 0) {
                bufferInfo = null;
                return false;
            }
            else {
                bufferInfo = new BufferInfo {
                    Buffer = _messageToSend,
                    Offset = _dataToSendOffset,
                    Length = Math.Min(dataSize, remainingBytesToSendCount)
                };
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

        public bool TryParse(BufferInfo bufferInfoRead, out ParsedData parsedData)
        {
            var usedToParseHeaderBytesCount = ParseHeader(bufferInfoRead);
            if (usedToParseHeaderBytesCount == bufferInfoRead.Length) {
                parsedData = ParsedData.None();
                return false;
            }

            byte[] message;

            var usedToParseMessageBytesCount = ParseMessage(
                bufferInfoRead.Buffer,
                bufferInfoRead.Length - usedToParseHeaderBytesCount,
                usedToParseHeaderBytesCount,
                out message
            );

            var remainingBytesCount = bufferInfoRead.Length - usedToParseHeaderBytesCount - usedToParseMessageBytesCount;
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
                    additionalData: bufferInfoRead.Buffer
                        .Skip(usedToParseHeaderBytesCount + usedToParseMessageBytesCount)
                        .ToArray()
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
        private int ParseHeader(BufferInfo bufferInfo)
        {
            if (_packageLength.HasValue) {
                return 0;
            }

            var remainingBytesCountForPackageLength = _packageLengthBytes.Length - _packageLengthReceivedBytesCount;
            if (bufferInfo.Length >= remainingBytesCountForPackageLength) {
                Buffer.BlockCopy(
                    bufferInfo.Buffer,
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
                    bufferInfo.Buffer,
                    0,
                    _packageLengthBytes,
                    _packageLengthReceivedBytesCount,
                    bufferInfo.Length);

                _packageLengthReceivedBytesCount += bufferInfo.Length;
                return bufferInfo.Length;
            }
        }
        private int ParseMessage(byte[] bytes, int bytesCount, int byteOffset, out byte[] message)
        {
            if (_packageLength.HasValue == false) {
                throw new Exception("Unable to process bytes received: the package length was not parsed.");
            }

            if (_receivedBytesCount + bytesCount == _packageLength) {
                Buffer.BlockCopy(bytes, byteOffset, _receivedMessage, _receivedBytesCount, bytesCount);
                message = _receivedMessage;
                _receivedBytesCount += bytesCount;
                return bytesCount;
            }
            if (_receivedBytesCount + bytesCount < _packageLength) {
                Buffer.BlockCopy(bytes, byteOffset, _receivedMessage, _receivedBytesCount, bytesCount);
                _receivedBytesCount += bytesCount;
                message = null;
                return bytesCount;
            }
            else {
                var remainingByteCount = _packageLength.Value - _receivedBytesCount;
                Buffer.BlockCopy(bytes, byteOffset, _receivedMessage, _receivedBytesCount, remainingByteCount);
                _receivedBytesCount += remainingByteCount;
                message = _receivedMessage;
                return remainingByteCount;
            }
        }
    }
}