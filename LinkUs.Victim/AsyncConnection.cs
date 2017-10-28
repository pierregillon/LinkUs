using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;

namespace LinkUs.Victim
{
    public class AsyncConnection
    {
        private readonly TcpClient _client;
        private readonly List<byte[]> _buffers = new List<byte[]>();

        public event Action<byte[]> DataReceived;

        protected virtual void OnDataReceived(byte[] data)
        {
            DataReceived?.Invoke(data);
        }

        public AsyncConnection(TcpClient client)
        {
            if (client == null) throw new ArgumentNullException(nameof(client));
            _client = client;
        }

        public void StartReceiving()
        {
            ReadDataAsync();
        }
        public void SendAsync(byte[] data)
        {
            var stream = _client.GetStream();
            stream.BeginWrite(data, 0, data.Length, OnDataWriten, _client);
        }

        private void ReadDataAsync()
        {
            var stream = _client.GetStream();
            var buffer = new byte[5];
            stream.BeginRead(buffer, 0, buffer.Length, OnDataRead, buffer);
        }

        private void OnDataRead(IAsyncResult result)
        {
            var network = _client.GetStream();
            var dataLength = network.EndRead(result);
            var data = (byte[]) result.AsyncState;
            _buffers.Add(data.Take(dataLength).ToArray());
            if (network.DataAvailable) {
                ReadDataAsync();
            }
            else {
                var fullData = _buffers.SelectMany(x => x).ToArray();
                _buffers.Clear();
                ReadDataAsync();
                OnDataReceived(fullData);
            }
        }
        private void OnDataWriten(IAsyncResult result)
        {
            _client.GetStream().EndWrite(result);
        }
    }
}