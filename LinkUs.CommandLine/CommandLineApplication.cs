using System;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.Core.Commands;

namespace LinkUs.CommandLine
{
    public class CommandLineApplication
    {
        private readonly IConsole _console;
        private readonly ICommandLineParser _parser;
        private readonly ICommandLineProcessor _processor;

        public CommandLineApplication(
            IConsole console,
            ICommandLineParser parser,
            ICommandLineProcessor processor)
        {
            _console = console;
            _parser = parser;
            _processor = processor;
        }

        public async Task Process(string[] arguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            try {
                var commandLine = _parser.Parse(arguments);
                await _processor.Process(commandLine);
            }
            catch (ArgumentParseException ex) {
                _console.WriteLine(ex.Message);
            }
            catch (Exception exception) {
                WriteException(exception);
            }
        }

        private void WriteException(Exception exception)
        {
            if (exception is AggregateException) {
                WriteException(((AggregateException) exception).InnerException);
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
                _console.WriteLineError(exception.ToString());
#else
                _console.WriteLineError(exception.Message);
#endif
            }
        }
    }
}