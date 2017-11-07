using System;
using System.Linq;
using System.Reflection;
using LinkUs.Core.Connection;
using LinkUs.Core.Modules.Commands;
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
                throw new Exception($"Unable to proces the command {commandName}.");
            }

            var commandInstance = _packageParser.Materialize(commandType, package);
            if (commandInstance is Ping) {
                return new PingHandler().Handle((Ping) commandInstance);
            }
            if (commandInstance is ListModules) {
                return GetModuleCommandHandler().Handle((ListModules) commandInstance);
            }
            if (commandInstance is LoadModule) {
                return GetModuleCommandHandler().Handle((LoadModule) commandInstance);
            }
            if (commandInstance is UnloadModule) {
                return GetModuleCommandHandler().Handle((UnloadModule) commandInstance);
            }
            throw new Exception("Handler not found");
        }

        private ModuleCommandHandler GetModuleCommandHandler()
        {
            return new ModuleCommandHandler(_moduleManager, _moduleLocator, _packageParser);
        }
        public void Dispose() { }
    }
}