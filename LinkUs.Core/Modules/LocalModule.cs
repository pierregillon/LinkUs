using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LinkUs.Core.PingLib;

namespace LinkUs.Core.Modules
{
    public class LocalModule : IModule
    {
        private IDictionary<string, MaterializationInfo> _infos = new ConcurrentDictionary<string, MaterializationInfo> {
            [typeof(Ping).Name] = new MaterializationInfo {CommandType = typeof(Ping), HandlerType = typeof(PingHandler)},
            [typeof(ListModules).Name] = new MaterializationInfo {CommandType = typeof(ListModules), HandlerType = typeof(ModuleCommandHandler)}
        };

        public ModuleInformation GetStatus()
        {
            return new ModuleInformation {
                Name = "Default",
                Version = "",
                IsLoaded = true
            };
        }
        public MaterializationInfo GetMaterializationInfo(string commandName)
        {
            MaterializationInfo info;
            if (_infos.TryGetValue(commandName, out info)) {
                return info;
            }
            throw new Exception("Cannot materialize.");
        }
        public bool CanProcess(string commandName)
        {
            return _infos.ContainsKey(commandName);
        }
    }
}