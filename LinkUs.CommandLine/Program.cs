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
            var tcpClient = new TcpClient();
            tcpClient.Connect("127.0.0.1", 9000);

            var commandDispatcher = new CommandDispatcher(tcpClient);

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
                        var pingResponse = commandDispatcher.Dispatch<string, string>("ping", targetId);
                        stopWatch.Stop();
                        Console.WriteLine($"Ok. {stopWatch.ElapsedMilliseconds} ms.");
                        break;
                    default:
                        var result = commandDispatcher.Dispatch<string, string>(command);
                        Console.WriteLine(result);
                        break;
                }
            }

            tcpClient.Close();
        }
    }
}