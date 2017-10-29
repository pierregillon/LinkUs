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
            var packageTransmitter = new PackageTransmitter(connection);
            packageTransmitter.PackageReceived += (sender, package) => {
                ProcessCommand(packageTransmitter, package);
            };

            Console.WriteLine("* Connected to client.");
            while (Console.ReadLine() != "exit") { }
            packageTransmitter.Close();
        }

        private static void ProcessCommand(PackageTransmitter transmitter, Package package)
        {
            Console.WriteLine(package);

            var command = Encoding.GetString(package.Content);
            if (command == "dir") {
                Thread.Sleep(1000);
                var packageResponse = package.CreateResponsePackage(Encoding.GetBytes(Directory.GetCurrentDirectory()));
                transmitter.Send(packageResponse);
            }
            else if (command == "date") {
                var packageResponse = package.CreateResponsePackage(Encoding.GetBytes(DateTime.Now.ToShortDateString()));
                transmitter.Send(packageResponse);
            }
            else if (command == "ping") {
                var packageResponse = package.CreateResponsePackage(Encoding.GetBytes("ok"));
                transmitter.Send(packageResponse);
            }
        }
    }
}