using System;
using LinkUs.CommandLine.ConsoleLib;

namespace LinkUs.CommandLine
{
    public class ConsoleCommandReader
    {
        private readonly IConsole _console;
        private readonly CommandLineApplication _commandLineApplication;

        // ----- Constructors
        public ConsoleCommandReader(
            IConsole console,
            CommandLineApplication commandLineApplication)
        {
            _console = console;
            _commandLineApplication = commandLineApplication;
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
            _commandLineApplication.Process(arguments).Wait();
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