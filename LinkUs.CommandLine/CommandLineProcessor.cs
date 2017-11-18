using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Modules.Default.ClientInformation;
using StructureMap;

namespace LinkUs.CommandLine
{
    public class CommandLineProcessor : ICommandLineProcessor
    {
        private readonly IContainer _container;
        private readonly IConsole _console;
        private readonly Parser _parser;

        // ----- Constructor
        public CommandLineProcessor(IContainer container, IConsole console, Parser parser)
        {
            _container = container;
            _console = console;
            _parser = parser;
        }

        // ----- Public methods
        public async Task Process(string[] arguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            try {
                object commandLine;
                if (TryParseArguments(arguments, out commandLine)) {
                    await ExecuteCommand(commandLine);
                }
            }
            catch (Exception exception) {
                WriteException(exception);
            }
        }
        private bool TryParseArguments(string[] arguments, out object commandLine)
        {
            var options = new Options();
            string verbName = null;
            object commandLineIntance = null;
            if (!_parser.ParseArguments(arguments, options, (verb, subOptions) => {
                verbName = verb;
                commandLineIntance = subOptions;
            })) {
                _console.WriteLine(HelpText.AutoBuild(options, verbName));
                commandLine = null;
                return false;
            }
            else {
                commandLine = commandLineIntance;
                return true;
            }
        }
        private Task ExecuteCommand(object commandLine)
        {
            var commandLineType = commandLine.GetType();
            var handlerContract = typeof(ICommandLineHandler<>).MakeGenericType(commandLineType);
            var handler = GetInstance(handlerContract);
            var handleMethod = GetHandleMethod(handlerContract, commandLineType);
            var task = (Task) handleMethod.Invoke(handler, new[] { commandLine });
            return task.ContinueWith(x => {
                //var connection = _container.TryGetInstance<IConnection>();
                //if (connection != null) {
                //    connection.Close();
                //    _container.Release(connection);
                //}
            });
        }
        private object GetInstance(Type handlerContract)
        {
            try {
                return _container.GetInstance(handlerContract);
            }
            catch (StructureMapConfigurationException ex) {
                if (ex.Message.Contains(typeof(IConnection).FullName)) {
                    var connection = ConnectToServer(_container);
                    _container.Inject(typeof(IConnection), connection);
                    var commandDispatcher = _container.GetInstance<CommandSender>();
                    commandDispatcher.ExecuteAsync(new SetStatus { Status = "Consumer" });
                    return GetInstance(handlerContract);
                }
                throw;
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

        // ----- Internal logic
        private static MethodInfo GetHandleMethod(Type handlerType, Type commandLineType)
        {
            return handlerType
                .GetMethods()
                .Where(x => x.Name == "Handle")
                .Single(x => x.GetParameters()[0].ParameterType == commandLineType);
        }
        private static IConnection ConnectToServer(IContainer container)
        {
            var globalParameters = container.GetInstance<GlobalParameters>();
            globalParameters.Load();
            var connector = container.GetInstance<Connector>();
            var connection = connector.Connect(globalParameters.ServerHost, globalParameters.ServerPort);
            return connection;
        }
    }
}