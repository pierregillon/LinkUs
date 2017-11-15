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
        public bool TryParse(byte[] bytesTransferred, out ParsedData parsedData)
        {
            var usedToParseHeaderBytesCount = ParseHeader(bytesTransferred);
            if (usedToParseHeaderBytesCount == bytesTransferred.Length) {
                parsedData = ParsedData.None();
                return false;
            }

            var messageBytes = bytesTransferred.SmartSkip(usedToParseHeaderBytesCount);

            byte[] message;
            var usedToParseMessageBytesCount = ParseMessage(messageBytes, out message);

            var remainingBytesCount = bytesTransferred.Length - usedToParseHeaderBytesCount - usedToParseMessageBytesCount;
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
        }

        // ----- Internal logic
        private int ParseHeader(byte[] bytes)
        {
            if (_packageLength.HasValue) {
                return 0;
            }

            var remainingBytesCountForPackageLength = _packageLengthBytes.Length - _packageLengthReceivedBytesCount;
            if (bytes.Length >= remainingBytesCountForPackageLength) {
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
                return remainingBytesCountForPackageLength;
            }
            else {
                Buffer.BlockCopy(
                    bytes,
                    0,
                    _packageLengthBytes,
                    _packageLengthReceivedBytesCount,
                    bytes.Length);

                _packageLengthReceivedBytesCount += bytes.Length;
                return bytes.Length;
            }
        }
        private int ParseMessage(byte[] bytes, out byte[] message)
        {
            if (_packageLength.HasValue == false) {
                throw new Exception("Unable to process bytes received: the package length was not parsed.");
            }

            var allBytesReceivedCount = _buffers.Select(x => x.Length).Sum(x => x);
            if (allBytesReceivedCount + bytes.Length == _packageLength) {
                _buffers.Add(bytes.ToArray());
                var fullMessage = _buffers.SelectMany(x => x).ToArray();
                message = fullMessage;
                return bytes.Length;
            }
            if (allBytesReceivedCount + bytes.Length < _packageLength) {
                _buffers.Add(bytes.ToArray());
                message = null;
                return bytes.Length;
            }
            else {
                var remainingByteCount = _packageLength.Value - allBytesReceivedCount;
                var exactBufferEnd = bytes.Take(remainingByteCount).ToArray();
                _buffers.Add(exactBufferEnd);
                var fullMessage = _buffers.SelectMany(x => x).ToArray();
                message = fullMessage;
                return remainingByteCount;
            }
        }
    }
}