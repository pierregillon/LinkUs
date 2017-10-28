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
            networkStream.Write(bytes, 0, bytes.Length);
        }
        private Package ReadPackage()
        {
            var buffer = new byte[200];
            var network = _tcpClient.GetStream();
            var buffers = new List<byte[]>();
            do {
                var bytesReceivedCount = network.Read(buffer, 0, buffer.Length);
                buffers.Add(buffer.Take(bytesReceivedCount).ToArray());
            } while (network.DataAvailable);
            return Package.Parse(buffers.SelectMany(x => x).ToArray());
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