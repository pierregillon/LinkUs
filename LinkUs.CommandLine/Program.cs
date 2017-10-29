using System;
using System.Collections.Generic;
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
            var connection = new SocketConnection();
            try {
                connection.Connect(host, port);
                ExecuteCommands(connection);
            }
            catch (Exception ex) {
                WriteInnerException(ex);
            }
            Console.Write("* Press any key to finish :");
            Console.ReadKey();
        }

        private static void ExecuteCommands(SocketConnection connection)
        {
            var packageTransmitter = new PackageTransmitter(connection);
            var commandDispatcher = new CommandDispatcher(packageTransmitter, new JsonSerializer());

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
                        var pingCommand = new Command() {Name = "ping"};
                        commandDispatcher.ExecuteAsync<Command, string>(pingCommand, targetId).Wait();
                        stopWatch.Stop();
                        Console.WriteLine($"Ok. {stopWatch.ElapsedMilliseconds} ms.");
                        break;
                    case "dir":
                        var result2 = ExecuteDir(commandDispatcher, arguments);
                        Console.WriteLine(result2);
                        break;
                    default:
                        var defaultCommand = new Command() {Name = command};
                        var result = commandDispatcher.ExecuteAsync<Command, string>(defaultCommand).Result;
                        Console.WriteLine(result);
                        break;
                }
            }
        }
        private static string ExecuteDir(CommandDispatcher commandDispatcher, string[] arguments)
        {
            var command = new ExecuteRemoteCommandLine {
                Name = "ExecuteRemoteCommandLine",
                CommandLine = "dir",
                Arguments = new List<object> {}
            };
            var targetId = ClientId.Parse(arguments[1]);
            var result = commandDispatcher.ExecuteAsync<ExecuteRemoteCommandLine, string>(command, targetId).Result;
            return result;
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