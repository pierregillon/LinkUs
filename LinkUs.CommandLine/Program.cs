﻿using System;
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
        static void Main(string[] arguments)
        {
            if (arguments.Any() == false) {
                var connection = CreateConnection();
                while (true) {
                    Console.Write("Command > ");
                    var command = Console.ReadLine();
                    arguments = command.Split(' ');
                    var commandResult = ExecuteCommand(connection, arguments);
                    Console.WriteLine(commandResult);
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
            var defaultCommand = new Command() {Name = "list-clients"};
            return commandDispatcher.ExecuteAsync<Command, string>(defaultCommand).Result;
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
                var pingCommand = new Command() {Name = "ping"};
                commandDispatcher.ExecuteAsync<Command, string>(pingCommand, targetId).Wait();
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
                ProcessShell(commandDispatcher, ClientId.Parse(target));
                return "";
            }
        }
        private static void ProcessShell(CommandDispatcher commandDispatcher, ClientId targetId)
        {
            Console.Write($"shell:{targetId}> ");
            var input = Console.ReadLine();
            var arguments = input.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var commandLine = arguments.First();
            var command = new ExecuteShellCommand {
                Name = "ExecuteShellCommand",
                CommandLine = commandLine,
                Arguments = arguments.Skip(1).OfType<object>().ToList()
            };
            commandDispatcher.ExecuteAsync(command, targetId);
            var driver = new ConsoleRemoteShellController(commandDispatcher.PackageTransmitter, targetId, new JsonSerializer());
            driver.SendInputs();
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