using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUs.Core.Connection
{
    public class BytesTransfertProtocol
    {
        private const int HEADER_LENGTH = 4;

        private readonly List<byte[]> _allDataReceived = new List<byte[]>();
        private int? _packageLength;
        private byte[] _packageLengthBytes = new byte[HEADER_LENGTH];
        private int _packageLengthReceivedBytesCount;

        // ----- Public methods
        public byte[] PrepareMessageToSend(byte[] data)
        {
            var fullData = new byte[_packageLengthBytes.Length + data.Length];
            _packageLengthBytes = BitConverter.GetBytes(data.Length);
            Buffer.BlockCopy(_packageLengthBytes, 0, fullData, 0, _packageLengthBytes.Length);
            Buffer.BlockCopy(data, 0, fullData, _packageLengthBytes.Length, data.Length);
            return fullData;
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
            _allDataReceived.Clear();
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

            var allBytesReceivedCount = _allDataReceived.Select(x => x.Length).Sum(x => x);
            if (allBytesReceivedCount + bytes.Length == _packageLength) {
                var fullMessage = _allDataReceived.SelectMany(x => x).Concat(bytes).ToArray();
                message = fullMessage;
                return bytes.Length;
            }
            if (allBytesReceivedCount + bytes.Length < _packageLength) {
                _allDataReceived.Add(bytes);
                message = null;
                return bytes.Length;
            }
            else {
                var remainingByteCount = _packageLength.Value - allBytesReceivedCount;
                var exactBufferEnd = bytes.Take(remainingByteCount).ToArray();
                var fullMessage = _allDataReceived.SelectMany(x => x).Concat(exactBufferEnd).ToArray();
                message = fullMessage;
                return remainingByteCount;
            }
        }
    }
}