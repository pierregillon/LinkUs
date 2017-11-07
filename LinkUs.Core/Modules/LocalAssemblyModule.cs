using System;
using System.Linq;
using System.Reflection;
using LinkUs.Core.Connection;
using LinkUs.Core.Modules.Commands;
using LinkUs.Core.Modules.Exceptions;
using LinkUs.Core.PingLib;

namespace LinkUs.Core.Modules
{
    public class LocalAssemblyModule : IModule
    {
        private readonly ModuleManager _moduleManager;
        private readonly ExternalAssemblyModuleLocator _moduleLocator;
        private readonly PackageParser _packageParser;

        public LocalAssemblyModule(
            ModuleManager moduleManager,
            ExternalAssemblyModuleLocator moduleLocator,
            PackageParser packageParser)
        {
            _moduleManager = moduleManager;
            _moduleLocator = moduleLocator;
            _packageParser = packageParser;
        }

        public string Name => GetType().Assembly.GetName().Name;
        public object Process(string commandName, Package package, IBus bus)
        {
            var commandType = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .SingleOrDefault(x => x.Name == commandName);

            if (commandType == null) {
                throw new UnknownCommandException(commandName, Name);
            }

            var commandInstance = _packageParser.Materialize(commandType, package);
            if (commandInstance is ListModules) {
                return GetModuleCommandHandler().Handle((ListModules) commandInstance);
            }
            if (commandInstance is LoadModule) {
                return GetModuleCommandHandler().Handle((LoadModule) commandInstance);
            }
            if (commandInstance is UnloadModule) {
                return GetModuleCommandHandler().Handle((UnloadModule) commandInstance);
            }
            var handlerType = Assembly
                .GetExecutingAssembly()
                .GetTypes()
                .SingleOrDefault(x => x.GetInterfaces().Any(y=> y.GetGenericArguments().Length != 0 && y.GetGenericArguments()[0] == commandType));

            var handler = Activator.CreateInstance(handlerType);
            var handle = handlerType
                .GetMethods()
                .Where(x => x.Name == "Handle")
                .Single(x => x.GetParameters()[0].ParameterType == commandType);

            return handle.Invoke(handler, new [] { commandInstance });
        }

        private ModuleCommandHandler GetModuleCommandHandler()
        {
            return new ModuleCommandHandler(_moduleManager, _moduleLocator, _packageParser);
        }
        public void Dispose() { }
    }
}