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
        public int ProcessHeader(byte[] bytes)
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
        public int ProcessMessage(byte[] bytes, Action<byte[]> dataReceived)
        {
            if (_packageLength.HasValue == false) {
                throw new Exception("Unable to process bytes received: the package length was not processed.");
            }

            var allBytesReceivedCount = _allDataReceived.Select(x => x.Length).Sum(x => x);
            if (allBytesReceivedCount + bytes.Length == _packageLength) {
                var fullMessage = _allDataReceived.SelectMany(x => x).Concat(bytes).ToArray();
                dataReceived?.Invoke(fullMessage);
                Reset();
                return bytes.Length;
            }
            if (allBytesReceivedCount + bytes.Length < _packageLength) {
                _allDataReceived.Add(bytes);
                return bytes.Length;
            }
            else {
                var remainingByteCount = _packageLength.Value - allBytesReceivedCount;
                var exactBufferEnd = bytes.Take(remainingByteCount).ToArray();
                var fullMessage = _allDataReceived.SelectMany(x => x).Concat(exactBufferEnd).ToArray();
                dataReceived?.Invoke(fullMessage);
                Reset();
                return remainingByteCount;
            }
        }
        public byte[] Transform(byte[] data)
        {
            var fullData = new byte[_packageLengthBytes.Length + data.Length];
            _packageLengthBytes = BitConverter.GetBytes(data.Length);
            Buffer.BlockCopy(_packageLengthBytes, 0, fullData, 0, _packageLengthBytes.Length);
            Buffer.BlockCopy(data, 0, fullData, _packageLengthBytes.Length, data.Length);
            return fullData;
        }
        public void Reset()
        {
            _allDataReceived.Clear();
            _packageLength = null;
            _packageLengthReceivedBytesCount = 0;
        }
    }
}