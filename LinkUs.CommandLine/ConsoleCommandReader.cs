using System;
using System.Linq;
using System.Reflection;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Modules.Default.ClientInformation;
using StructureMap;

namespace LinkUs.CommandLine
{
    public class ConsoleCommandReader
    {
        private readonly IConsole _console;
        private readonly ICommandLineProcessor _commandLineProcessor;

        // ----- Constructors
        public ConsoleCommandReader(
            IConsole console,
            ICommandLineProcessor commandLineProcessor)
        {
            _console = console;
            _commandLineProcessor = commandLineProcessor;
        }

        // ----- Public methods
        public void ExecuteSingleCommand(string[] arguments)
        {
            ProcessCommand(arguments);
        }
        public void ExecuteMultipleCommands()
        {
            WhileReadingCommands(ProcessCommand);
        }

        // ----- Internal logic
        private void ProcessCommand(string[] arguments)
        {
            try {
                _commandLineProcessor.Process(arguments).Wait();
            }
            catch (Exception ex) {
                WriteException(ex);
            }
        }
        private void WriteException(Exception exception)
        {
            if (exception is AggregateException) {
                WriteException(((AggregateException)exception).InnerException);
                return;
            }

            if (exception is ErrorOccuredOnRemoteClientException) {
                var remoteException = (ErrorOccuredOnRemoteClientException) exception;
                _console.WriteLineError("An unexpected exception occurred on the remote client.");
#if DEBUG
                _console.WriteLineError(remoteException.FullMessage);
#else
                _console.WriteLineError(remoteException.Message);
#endif
            }
            else {
                _console.WriteLineError("An unexpected exception occurred during the command process.");
#if DEBUG
                _console.WriteLineError(exception.StackTrace);
#else
                _console.WriteLineError(exception.Message);
#endif
            }
        }
        private void WhileReadingCommands(Action<string[]> action)
        {
            while (true) {
                _console.Write("> ");
                var command = _console.ReadLine();
                if (string.IsNullOrEmpty(command) == false) {
                    if (command == "exit") {
                        break;
                    }
                    var commandArguments = command.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    action(commandArguments);
                }
            }
        }
    }
}