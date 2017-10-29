using System;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using LinkUs.Core;

namespace LinkUs.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect("127.0.0.1", 9000);

            var connection = new SocketConnection(socket);
            var connector = new PackageConnector(connection);
            var commandDispatcher = new CommandDispatcher(connector);

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

            connector.Close();
        }
    }
}