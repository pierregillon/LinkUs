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

            var identificationPackage = ReadPackage(tcpClient);
            var clientId = identificationPackage.Destination;

            var commandLine = "";
            while (commandLine != "exit") {
                Console.Write("Command: ");
                commandLine = Console.ReadLine();
                var arguments = commandLine.Split(' ');
                var command = arguments.First();
                string result = "";
                switch (command) {
                    case "ping":
                        var stopWatch = new Stopwatch();
                        var target = arguments[1];
                        var targetId = ClientId.Parse(target);
                        var package = new Package(clientId, targetId, Encoding.GetBytes("ping"));
                        stopWatch.Start();
                        SendPackage(tcpClient, package);
                        var packageResponse = ReadPackage(tcpClient);
                        stopWatch.Stop();
                        result = Encoding.GetString(packageResponse.Content);
                        if (result == "ok") {
                            Console.WriteLine($"Ok. {stopWatch.ElapsedMilliseconds} ms.");
                        }
                        break;
                    default:
                        result = ExecuteCommandLine(tcpClient, commandLine, clientId);
                        Console.WriteLine(result);
                        break;
                }
            }

            tcpClient.Close();
        }
        private static string ExecuteCommandLine(TcpClient tcpClient, string commandLine, ClientId clientId)
        {
            var package = new Package(clientId, ClientId.Server, Encoding.GetBytes(commandLine));
            SendPackage(tcpClient, package);

            var packageResponse = ReadPackage(tcpClient);
            return Encoding.GetString(packageResponse.Content);
        }
        private static void SendPackage(TcpClient tcpClient, Package package)
        {
            var bytes = package.ToByteArray();
            var networkStream = tcpClient.GetStream();
            networkStream.Write(bytes, 0, bytes.Length);
        }

        private static Package ReadPackage(TcpClient tcpClient)
        {
            var buffer = new byte[200];
            var network = tcpClient.GetStream();
            var buffers = new List<byte[]>();
            do {
                var bytesReceivedCount = network.Read(buffer, 0, buffer.Length);
                buffers.Add(buffer.Take(bytesReceivedCount).ToArray());
            } while (network.DataAvailable);
            return Package.Parse(buffers.SelectMany(x => x).ToArray());
        }
    }
}