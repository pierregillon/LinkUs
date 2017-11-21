using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Modules.Default.ClientInformation;
using StructureMap;

namespace LinkUs.CommandLine
{
    public class CommandLineDispatcher : ICommandLineDispatcher
    {
        private readonly IContainer _container;

        // ----- Constructor
        public CommandLineDispatcher(IContainer container)
        {
            _container = container;
        }

        // ----- Public methods
        public Task Dispatch(object commandLine)
        {
            ConnectToServerIfNeeded(commandLine);
            return HandleCommandLine(commandLine)
                .ContinueWith(CloseConnectionIfNeeded);
        }
        private void ConnectToServerIfNeeded(object commandLine)
        {
            try {
                var commandLineType = commandLine.GetType();
                var handlerContract = typeof(ICommandLineHandler<>).MakeGenericType(commandLineType);
                _container.GetInstance(handlerContract);
            }
            catch (StructureMapConfigurationException ex) {
                var fullName = typeof(IConnection).FullName;
                if (ex.Message.Contains(fullName)) {
                    ConnectToServer(_container);
                    return;
                }
                throw;
            }
        }
        private Task HandleCommandLine(object commandLine)
        {
            var commandLineType = commandLine.GetType();
            var handlerContract = typeof(ICommandLineHandler<>).MakeGenericType(commandLineType);
            var handler = _container.GetInstance(handlerContract);
            var handleMethod = GetHandleMethod(handlerContract, commandLineType);
            var task = (Task) handleMethod.Invoke(handler, new[] { commandLine });
            return task;
        }
        private void CloseConnectionIfNeeded(Task task)
        {
            var connection = _container.TryGetInstance<IConnection>();
            if (connection != null) {
                connection.Close();
                _container.Release(connection);
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
        private static void ConnectToServer(IContainer container)
        {
            var globalParameters = container.GetInstance<GlobalParameters>();
            globalParameters.Load();
            var connector = container.GetInstance<Connector>();
            var connection = connector.Connect(globalParameters.ServerHost, globalParameters.ServerPort);
            container.Inject(typeof(IConnection), connection);
            var commandDispatcher = container.GetInstance<CommandSender>();
            commandDispatcher.ExecuteAsync(new SetStatus { Status = "Consumer" });
        }
    }
}