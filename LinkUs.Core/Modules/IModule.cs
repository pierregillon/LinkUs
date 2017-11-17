using System;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Core.Packages;

namespace LinkUs.Core.Modules
{
    public interface IModule : IDisposable
    {
        string Name { get; }
        object Process(string commandName, Package package, IBus bus);
    }
}