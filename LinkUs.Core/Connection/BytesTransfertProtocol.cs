using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Lifetime;

namespace LinkUs.Core.Connection
{
    public class BytesTransfertProtocol
    {
        private const int HEADER_LENGTH = 4;

        private readonly List<byte[]> _buffers = new List<byte[]>();
        private int? _packageLength;
        private byte[] _packageLengthBytes = new byte[HEADER_LENGTH];
        private int _packageLengthReceivedBytesCount;
        private byte[] _receivedMessage;
        private int _receivedBytesCount;

        // ----- Public methods
        public byte[] PrepareMessageToSend(byte[] data, int bufferLength = 1024)
        {
            var fullData = new byte[_packageLengthBytes.Length + data.Length];
            _packageLengthBytes = BitConverter.GetBytes(data.Length);
            Buffer.BlockCopy(_packageLengthBytes, 0, fullData, 0, _packageLengthBytes.Length);
            Buffer.BlockCopy(data, 0, fullData, _packageLengthBytes.Length, data.Length);

            for (var i = 0; i < fullData.Length / bufferLength + 1; i++) {
                if (fullData.Length - i * bufferLength > bufferLength) {
                    _buffers.Add(fullData.Skip(i * bufferLength).Take(bufferLength).ToArray());
                }
                else {
                    _buffers.Add(fullData.Skip(i * bufferLength).Take(fullData.Length - i * bufferLength).ToArray());
                }
            }

            var first = _buffers.First();
            _buffers.RemoveAt(0);
            return first;
        }
        public bool TryGetNextBytes(out byte[] nextBytesToSend)
        {
            if (_buffers.Count == 0) {
                nextBytesToSend = null;
                return false;
            }
            else {
                var first = _buffers.First();
                _buffers.RemoveAt(0);
                nextBytesToSend = first;
                return true;
            }
        }
        public bool TryParse(byte[] bytesTransferred, int bytesTransferredCount, out ParsedData parsedData)
        {
            var usedToParseHeaderBytesCount = ParseHeader(bytesTransferred, bytesTransferredCount);
            if (usedToParseHeaderBytesCount == bytesTransferredCount) {
                parsedData = ParsedData.None();
                return false;
            }

            byte[] message;

            var usedToParseMessageBytesCount = ParseMessage(
                bytesTransferred, 
                bytesTransferredCount - usedToParseHeaderBytesCount, 
                usedToParseHeaderBytesCount, 
                out message
            );

            var remainingBytesCount = bytesTransferredCount - usedToParseHeaderBytesCount - usedToParseMessageBytesCount;
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
                    additionalData: bytesTransferred
                        .Skip(usedToParseHeaderBytesCount + usedToParseMessageBytesCount)
                        .ToArray()
                );
                return true;
            }
        }
        public void Reset()
        {
            _buffers.Clear();
            _packageLength = null;
            _packageLengthReceivedBytesCount = 0;
            _receivedBytesCount = 0;
            _receivedMessage = null;
        }

        // ----- Internal logic
        private int ParseHeader(byte[] bytes, int bytesCount)
        {
            if (_packageLength.HasValue) {
                return 0;
            }

            var remainingBytesCountForPackageLength = _packageLengthBytes.Length - _packageLengthReceivedBytesCount;
            if (bytesCount >= remainingBytesCountForPackageLength) {
                Buffer.BlockCopy(
                    bytes,
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
                    bytes,
                    0,
                    _packageLengthBytes,
                    _packageLengthReceivedBytesCount,
                    bytesCount);

                _packageLengthReceivedBytesCount += bytesCount;
                return bytesCount;
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