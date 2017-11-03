using System;
using System.Diagnostics;
using System.Linq;
using Fclp;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;

namespace LinkUs.CommandLine
{
    class Program
    {
        static void Main(string[] arguments)
        {
            if (arguments.Any() == false) {
                var connection = CreateConnection();
                while (true) {
                    Console.Write("> ");
                    var command = Console.ReadLine();
                    if (string.IsNullOrEmpty(command) == false) {
                        var commandArguments = command.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        var commandResult = ExecuteCommand(connection, commandArguments);
                        Console.WriteLine(commandResult);
                    }
                }
            }
            else {
                var connection = CreateConnection();
                var commandResult = ExecuteCommand(connection, arguments);
                Console.WriteLine(commandResult);
            }
        }

        // ----- Internal logics
        private static string ExecuteCommand(IConnection connection, string[] arguments)
        {
            var commandDispatcher = GetCommandDispatcher(connection);
            var command = arguments.First();
            string commandResult = "";
            try {
                switch (command) {
                    case "ping":
                        commandResult = Ping(commandDispatcher, arguments.Skip(1).ToArray());
                        break;
                    case "list-clients":
                        commandResult = ListClients(commandDispatcher);
                        break;
                    case "shell":
                        commandResult = Shell(commandDispatcher, arguments.Skip(1).ToArray());
                        break;
                    default:
                        commandResult = $"'{command}' is not recognized as a command.";
                        break;
                }
            }
            catch (Exception ex) {
                WriteInnerException(ex);
            }
            return commandResult;
        }

        // ----- Commands
        private static string ListClients(CommandDispatcher commandDispatcher)
        {
            var defaultCommand = new ListRemoteClients();
            return commandDispatcher.ExecuteAsync<ListRemoteClients, string>(defaultCommand).Result;
        }
        private static string Ping(CommandDispatcher commandDispatcher, string[] arguments)
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
                var targetId = ClientId.Parse(target);
                var stopWatch = new Stopwatch();
                stopWatch.Start();
                var pingCommand = new Ping();
                commandDispatcher.ExecuteAsync<Ping, string>(pingCommand, targetId).Wait();
                stopWatch.Stop();
                return $"Ok. {stopWatch.ElapsedMilliseconds} ms.";
            }
        }
        private static string Shell(CommandDispatcher commandDispatcher, string[] arguments)
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
                var targetId = ClientId.Parse(target);
                var driver = new ConsoleRemoteShellController(commandDispatcher, targetId, new JsonSerializer());
                driver.SendInputs();
                return "";
            }
        }

        // ----- Utils
        private static CommandDispatcher GetCommandDispatcher(IConnection connection)
        {
            var packageTransmitter = new PackageTransmitter(connection);
            return new CommandDispatcher(packageTransmitter, new JsonSerializer());
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
        private static IConnection CreateConnection()
        {
            string host = "127.0.0.1";
            int port = 9000;
            var connection = new SocketConnection();
            connection.Connect(host, port);
            return connection;
        }
    }
}