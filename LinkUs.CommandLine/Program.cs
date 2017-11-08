﻿using System;
using System.Linq;
using System.Reflection;
using CommandLine;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.Core;
using LinkUs.Core.ClientInformation;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using StructureMap;

namespace LinkUs.CommandLine
{
    class Program
    {
        static void Main(string[] arguments)
        {
            var container = BuildContainer();
            if (arguments != null && arguments.Any()) {
                ExecuteSingleCommand(container, arguments);
            }
            else {
                ExecuteMultipleCommands(container);
            }
        }

        // ----- Internal logics
        private static void ExecuteSingleCommand(IContainer container, string[] arguments)
        {
            var connection = ConnectToServer(container);
            try {
                ProcessCommand(container, arguments);
            }
            finally {
                connection.Close();
            }
        }
        private static void ExecuteMultipleCommands(IContainer container)
        {
            var connection = ConnectToServer(container);
            try {
                WhileReadingCommands(args => {
                    ProcessCommand(container, args);
                });
            }
            finally {
                connection.Close();
            }
        }
        private static void ProcessCommand(IContainer container, string[] arguments)
        {
            var processor = container.GetInstance<ICommandLineProcessor>();
            try {
                processor.Process(arguments);
            }
            catch (CommandLineProcessingFailed ex) {
                var console = container.GetInstance<IConsole>();
                console.Write(ex.Message);
            }
            catch (TargetInvocationException ex) {
                var console = container.GetInstance<IConsole>();
                WriteInnerException(console, ex.InnerException);
            }
        }
        private static void WhileReadingCommands(Action<string[]> action)
        {
            while (true) {
                Console.Write("> ");
                var command = Console.ReadLine();
                if (string.IsNullOrEmpty(command) == false) {
                    if (command == "exit") {
                        break;
                    }
                    var commandArguments = command.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
                    action(commandArguments);
                }
            }
        }

        // ----- Utils
        private static Container BuildContainer()
        {
            return new Container(configuration => {
                configuration.For<ICommandLineProcessor>().Use<CommandLineProcessor>();
                configuration.For<CommandDispatcher>();
                configuration.For<IConsole>().Use<WindowsConsole>();
                configuration.For<ISerializer>().Use<JsonSerializer>();
                configuration.For<Parser>().Use(Parser.Default);

                configuration.Scan(y => {
                    y.TheCallingAssembly();
                    y.AddAllTypesOf(typeof(IHandler<>));
                    y.WithDefaultConventions();
                });
            });
        }
        private static void WriteInnerException(IConsole console, Exception exception)
        {
            if (exception is AggregateException) {
                WriteInnerException(console, ((AggregateException) exception).InnerException);
            }
            else {
                console.WriteLineError(exception.Message);
            }
        }
        private static IConnection ConnectToServer(IContainer container)
        {
            var host = "127.0.0.1";
            var port = 9000;
            var connection = new SocketConnection();
            connection.Connect(host, port);
            container.Inject(typeof(IConnection), connection);
            var commandDispatcher = container.GetInstance<CommandDispatcher>();
            commandDispatcher.ExecuteAsync(new SetStatus {Status = "Consumer"});
            return connection;
        }
    }
}