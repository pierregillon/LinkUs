using System;
using LinkUs.Core.Connection;

namespace LinkUs.Core.Modules
{
    public interface IModule : IDisposable
    {
        string Name { get; }
        object Process(string commandName, Package package, IBus bus);
    }
}