using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using LinkUs.Core;

namespace LinkUs.CommandLine
{
    class Program
    {
        static readonly UTF8Encoding Encoding = new UTF8Encoding();

        static void Main(string[] args)
        {
            var tcpClient = new TcpClient();
            tcpClient.Connect("127.0.0.1", 9000);

            var commandDispatcher = new CommandDispatcher(tcpClient);

            var commandLine = "";
            while (commandLine != "exit") {
                Console.Write("Command: ");
                commandLine = Console.ReadLine();
                var arguments = commandLine.Split(' ');
                var command = arguments.First();
                string result = "";
                switch (command) {
                    case "ping":
                        var targetId = ClientId.Parse(arguments[1]);
                        var stopWatch = new Stopwatch();
                        stopWatch.Start();
                        var pingResponse = commandDispatcher.Dispatch<string, string>("ping", targetId);
                        stopWatch.Stop();
                        Console.WriteLine($"Ok. {stopWatch.ElapsedMilliseconds} ms.");
                        break;
                    default:
                        result = commandDispatcher.Dispatch<string, string>(command);
                        Console.WriteLine(result);
                        break;
                }
            }

            tcpClient.Close();
        }
    }

    public class CommandDispatcher
    {
        private readonly TcpClient _tcpClient;
        private readonly ClientId _currentClientId;

        public CommandDispatcher(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;

            var identificationPackage = ReadPackage();
            _currentClientId = identificationPackage.Destination;
        }

        public TResult Dispatch<TCommand, TResult>(TCommand command, ClientId clientId = null)
        {
            clientId = clientId ?? ClientId.Server;
            var commandPackage = new Package(_currentClientId, clientId, Serialize(command));
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