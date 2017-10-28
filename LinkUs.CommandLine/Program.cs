using System;
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

            var clientIdBuffer = new byte[200];
            var bytesReceivedCount = tcpClient.GetStream().Read(clientIdBuffer, 0, clientIdBuffer.Length);
            var identificationPackage = Package.Parse(clientIdBuffer.Take(bytesReceivedCount).ToArray());
            var clientId = identificationPackage.Destination;
            
            var commandLine = "";
            while (commandLine != "exit") {
                Console.Write("Command: ");
                commandLine = Console.ReadLine();
                var package = new Package(clientId, ClientId.Server, Encoding.GetBytes(commandLine));
                var bytes = package.ToByteArray();
                tcpClient.GetStream().Write(bytes, 0, bytes.Length);

                var buffer = new byte[1024];
                var count = tcpClient.GetStream().Read(buffer, 0, buffer.Length);
                var packageResponse = Package.Parse(buffer.Take(count).ToArray());
                Console.WriteLine(Encoding.GetString(packageResponse.Content));
            }
        }
    }
}