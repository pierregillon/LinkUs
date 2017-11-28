using System;
using System.Reflection;

namespace LinkUs.Client.Infrastructure
{
    public class ClientEnvironment : IEnvironment
    {
        public ClientEnvironment()
        {
            ApplicationPath = Assembly.GetExecutingAssembly().Location;
        }

        public string ApplicationPath { get; }
        public bool Is64Bit => Environment.Is64BitOperatingSystem;
        public Version CurrentVersion => Assembly.GetEntryAssembly().GetName().Version;
    }
}