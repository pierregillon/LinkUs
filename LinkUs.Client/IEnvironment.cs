using System;

namespace LinkUs.Client
{
    public interface IEnvironment
    {
        string ApplicationPath { get; }
        bool Is64Bit { get; }
        Version CurrentVersion { get; }
    }
}