using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using LinkUs.Core;

namespace LinkUs.CommandLine
{
    public class CommandDispatcher
    {
        private readonly TcpClient _tcpClient;

        public CommandDispatcher(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
        }

        public TResult Dispatch<TCommand, TResult>(TCommand command, ClientId clientId = null)
        {
            clientId = clientId ?? ClientId.Server;
            var commandPackage = new Package(ClientId.Unknown, clientId, Serialize(command));
            SendPackage(commandPackage);
            var resultPackage = ReadPackage();
            return Deserialize<TResult>(resultPackage.Content);
        }

        private void SendPackage(Package package)
        {
            var bytes = package.ToByteArray();
            var networkStream = _tcpClient.GetStream();
            var lengthBytes = BitConverter.GetBytes(bytes.Length);
            var allBytes = new byte[bytes.Length + lengthBytes.Length];
            Buffer.BlockCopy(lengthBytes, 0, allBytes, 0, lengthBytes.Length);
            Buffer.BlockCopy(bytes, 0, allBytes, lengthBytes.Length, bytes.Length);
            networkStream.Write(allBytes, 0, allBytes.Length);
        }
        private Package ReadPackage()
        {
            var network = _tcpClient.GetStream();
            var lengthBytes = new byte[4];
            network.Read(lengthBytes, 0, lengthBytes.Length);
            var length = BitConverter.ToInt32(lengthBytes, 0);
            var buffer = new byte[200];
            var finalBuffer = new byte[length];
            var readCount = 0;
            do {
                var load = buffer.Length;
                if (load > length - readCount) {
                    load = length - readCount;
                }
                var bytesReceivedCount = network.Read(finalBuffer, 0, load);
                readCount += bytesReceivedCount;
            } while (readCount < length);
            return Package.Parse(finalBuffer);
        }
        private static byte[] Serialize<T>(T command)
        {
            if (command is string) {
                return Encoding.UTF8.GetBytes((string) (object) command);
            }
            throw new NotImplementedException();
        }
        private static T Deserialize<T>(byte[] result)
        {
            return (T) (object) Encoding.UTF8.GetString(result);
        }
    }
}