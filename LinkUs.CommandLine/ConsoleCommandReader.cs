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
            catch (InvalidCommandLineArguments ex) {
                _console.Write(ex.Message);
            }
            catch (TargetInvocationException ex) {
                WriteInnerException(ex.InnerException);
            }
            catch (Exception ex) {
                WriteInnerException(ex);
            }
            finally {
                _console.NewLine();
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
        private void WriteInnerException(Exception exception)
        {
            if (exception is AggregateException) {
                WriteInnerException(((AggregateException) exception).InnerException);
            }
            else {
                _console.WriteLineError(exception.ToString());
            }
        }
    }
}