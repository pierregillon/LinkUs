using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using LinkUs.Core;

namespace LinkUs.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            string host = "127.0.0.1";
            int port = 9000;

            Console.WriteLine($"* Searching for host {host} on port {port}.");
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try {
                socket.Connect(host, port);
                ExecuteCommands(socket);
            }
            catch (Exception ex) {
                WriteInnerException(ex);
            }
            Console.Write("* Press any key to finish :");
            Console.ReadKey();
        }

        private static void ExecuteCommands(Socket socket)
        {
            var connection = new SocketConnection(socket);
            var packageTransmitter = new PackageTransmitter(connection);
            var commandDispatcher = new CommandDispatcher(packageTransmitter);

            var commandLine = "";
            while (commandLine != "exit") {
                Console.Write("Command: ");
                commandLine = Console.ReadLine();
                var arguments = commandLine.Split(' ');
                var command = arguments.First();
                switch (command) {
                    case "ping":
                        var targetId = ClientId.Parse(arguments[1]);
                        var stopWatch = new Stopwatch();
                        stopWatch.Start();
                        var pingResponse = commandDispatcher.ExecuteAsync<string, string>("ping", targetId).Result;
                        stopWatch.Stop();
                        Console.WriteLine($"Ok. {stopWatch.ElapsedMilliseconds} ms.");
                        break;
                    default:
                        var result = commandDispatcher.ExecuteAsync<string, string>(command).Result;
                        Console.WriteLine(result);
                        break;
                }
            }
        }

        private static void WriteInnerException(Exception exception)
        {
            if (exception is AggregateException) {
                WriteInnerException(((AggregateException) exception).InnerException);
            }
            else {
                Console.WriteLine(exception);
            }
        }
    }
}