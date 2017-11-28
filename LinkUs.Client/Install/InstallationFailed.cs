using System;

namespace LinkUs.Client.Install
{
    public class InstallationFailed : Exception
    {
        public InstallationFailed(Exception exception) : base("Installation failed.", exception) { }
    }
}