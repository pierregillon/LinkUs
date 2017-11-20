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
    public class CommandLineProcessor : ICommandLineProcessor
    {
        private readonly IContainer _container;

        // ----- Constructor
        public CommandLineProcessor(IContainer container)
        {
            _container = container;
        }

        // ----- Public methods
        public async Task Process(object commandLine)
        {
            await ExecuteCommand(commandLine);
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