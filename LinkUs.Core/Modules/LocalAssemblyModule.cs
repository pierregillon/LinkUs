using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LinkUs.Core.Connection;
using LinkUs.Core.Modules.Commands;
using LinkUs.Core.PingLib;

namespace LinkUs.Core.Modules
{
    public class LocalAssemblyModule : IModule
    {
        private readonly ModuleManager _moduleManager;
        private readonly PackageParser _packageParser;

        private readonly IDictionary<string, MaterializationInfo> _infos = new ConcurrentDictionary<string, MaterializationInfo> {
            [typeof(Ping).Name] = new MaterializationInfo {CommandType = typeof(Ping), HandlerType = typeof(PingHandler)},
            [typeof(ListModules).Name] = new MaterializationInfo {CommandType = typeof(ListModules), HandlerType = typeof(ModuleCommandHandler)},
            [typeof(LoadModule).Name] = new MaterializationInfo {CommandType = typeof(LoadModule), HandlerType = typeof(ModuleCommandHandler)}
        };

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
            var commandType = _infos[commandName];
            var commandInstance = _packageParser.Materialize(commandType.CommandType, package);
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