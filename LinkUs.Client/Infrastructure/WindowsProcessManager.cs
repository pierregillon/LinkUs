using System;
using System.ComponentModel;
using System.Diagnostics;

namespace LinkUs.Client.Infrastructure
{
    public class WindowsProcessManager : IProcessManager
    {
        public bool StartProcessWithElevatedPrivileges(string exeFilePath)
        {
            try {
                var startInfo = new ProcessStartInfo(exeFilePath) {
                    Verb = "runas"
                };
                Process.Start(startInfo);
                return true;
            }
            catch (Win32Exception) {
                return false;
            }
        }
    }
}