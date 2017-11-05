using System;

namespace LinkUs.Core.Modules
{
    public interface IModule
    {
        ModuleInformation GetStatus();
        MaterializationInfo GetMaterializationInfo(string commandName);
        bool CanProcess(string commandName);
    }

    public class MaterializationInfo
    {
        public Type HandlerType { get; set; }
        public Type CommandType { get; set; }
    }
}