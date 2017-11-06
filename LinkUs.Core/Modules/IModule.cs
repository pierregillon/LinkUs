using System;
using LinkUs.Core.Connection;

namespace LinkUs.Core.Modules
{
    public interface IModule
    {
        string Name { get; }
        object Process(string commandName, Package package, IBus bus);
        ModuleInformation GetStatus();
    }

    public class MaterializationInfo
    {
        public Type HandlerType { get; set; }
        public Type CommandType { get; set; }
    }
}