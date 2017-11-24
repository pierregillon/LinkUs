using System;
using LinkUs.Core.Commands;
using LinkUs.Core.Packages;

namespace LinkUs.Modules.Default.Modules
{
    public interface IModule : IDisposable
    {
        string Name { get; }
        string Version { get; }
        string FileLocation { get; }

        object Process(string commandName, Package package, IBus bus);
    }
}