using System;
using System.Collections.Generic;

namespace LinkUs.Core.Modules
{
    public interface IModule
    {
        IEnumerable<Type> AvailableHandlers { get; }
        IEnumerable<Type> AvailableCommands { get; }
        ModuleInformation GetStatus();
    }
}