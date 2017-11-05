using System;
using System.Collections.Generic;
using LinkUs.Core.PingLib;

namespace LinkUs.Core.Modules
{
    public class LocalModule : IModule
    {
        public IEnumerable<Type> AvailableHandlers
        {
            get
            {
                yield return typeof(PingHandler);
                yield return typeof(ModuleCommandHandler);
            }
        }

        public IEnumerable<Type> AvailableCommands
        {
            get
            {
                yield return typeof(Ping);
                yield return typeof(ListModules);
            }
        }

        public ModuleInformation GetStatus()
        {
            return new ModuleInformation {
                Name = "Default",
                Version = "",
                IsLoaded = true
            };
        }
    }
}