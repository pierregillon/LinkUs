using System;
using LinkUs.CommandLine.ConsoleLib;

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
            _commandLineProcessor.Process(arguments).Wait();
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
                    _console.NewLine();
                }
            }
        }
    }
}