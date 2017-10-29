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

        static void Main(string[] args)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("127.0.0.1", 9000);

            var connection = new SocketConnection(socket);
            var connector = new PackageConnector(connection);
            connector.PackageReceived += (sender, package) => {
                ProcessCommand(connector, package);
            };

            Console.WriteLine("* Connected to client.");
            while (Console.ReadLine() != "exit") { }
            connector.Close();
        }

        private static void ProcessCommand(PackageConnector connector, Package package)
        {
            Console.WriteLine(package);

            var command = Encoding.GetString(package.Content);
            if (command == "dir") {
                Thread.Sleep(1000);
                var packageResponse = package.CreateResponsePackage(Encoding.GetBytes(Directory.GetCurrentDirectory()));
                connector.Send(packageResponse);
            }
            else if (command == "date") {
                var packageResponse = package.CreateResponsePackage(Encoding.GetBytes(DateTime.Now.ToShortDateString()));
                connector.Send(packageResponse);
            }
            else if (command == "ping") {
                var packageResponse = package.CreateResponsePackage(Encoding.GetBytes("ok"));
                connector.Send(packageResponse);
            }
        }
    }
}