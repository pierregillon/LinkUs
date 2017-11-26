using System;
using System.Reflection;
using LinkUs.Modules.Default.Install;

namespace LinkUs.Client
{
    public class ClientEnvironment : IEnvironment
    {
        public ClientEnvironment()
        {
            ApplicationPath = Assembly.GetExecutingAssembly().Location;
        }

        public string ApplicationPath { get; }
        public string InstallationDirectory
        {
            get
            {
                // For 64bit operating system, when trying to copy file to
                // system32, it copies file in C:\WINDOWS\SysWOW64 because of
                // file redirector. (system32 directory contains only 64bit programs.)
                if (Environment.Is64BitOperatingSystem) {
                    return Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
                }
                else {
                    return Environment.GetFolderPath(Environment.SpecialFolder.System);
                }
            }
        }
    }
}