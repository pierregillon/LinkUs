﻿using System;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
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
                        if (command == "exit") {
                            break;
                        }
                        var commandArguments = command.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                        ParseArguments(commandArguments, connection);
                    }
                }
                connection.Close();
            }
            else {
                var connection = CreateConnection();
                ParseArguments(arguments, connection);
                connection.Close();
            }
        }
        private static void ParseArguments(string[] commandArguments, IConnection connection)
        {
            var options = new Options();
            string invokedVerb = null;
            object invokedVerbInstance = null;
            if (!Parser.Default.ParseArguments(commandArguments, options, (verb, subOptions) => {
                invokedVerb = verb;
                invokedVerbInstance = subOptions;
            })) {
                Console.WriteLine(HelpText.AutoBuild(options, invokedVerb));
            }
            else {
                ExecuteCommand(connection, invokedVerbInstance);
            }
        }

        // ----- Internal logics
        private static void ExecuteCommand(IConnection connection, object commandLine)
        {
            try {
                var commandDispatcher = GetCommandDispatcher(connection);
                var commandLineType = commandLine.GetType();
                var commandLineHandlerType =
                    Assembly.GetExecutingAssembly()
                        .GetTypes()
                        .SingleOrDefault(x => x.GetInterfaces().Contains(typeof(IHandler<>).MakeGenericType(commandLineType)));
                if (commandLineHandlerType == null) {
                    throw new Exception("No handler found for the command");
                }

                var commandLineHandler = Activator.CreateInstance(commandLineHandlerType, commandDispatcher);
                var handleMethod = commandLineHandlerType
                    .GetMethods()
                    .Where(x => x.Name == "Handle")
                    .Single(x => x.GetParameters()[0].ParameterType == commandLineType);
                handleMethod.Invoke(commandLineHandler, new[] {commandLine});
            }
            catch (Exception ex) {
                WriteInnerException(ex);
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