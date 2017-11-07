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
        private readonly PackageParser _packageParser;

        public LocalAssemblyModule(ModuleManager moduleManager, PackageParser packageParser)
        {
            _moduleManager = moduleManager;
            _packageParser = packageParser;
        }

        public ModuleInformation GetStatus()
        {
            return new ModuleInformation {
                Name = "Default",
                Version = "",
                IsLoaded = true
            };
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
                return new ModuleCommandHandler(_moduleManager).Handle((ListModules) commandInstance);
            }
            if (commandInstance is LoadModule) {
                return new ModuleCommandHandler(_moduleManager).Handle((LoadModule) commandInstance);
            }
            throw new Exception("Handler not found");
        }
    }
}