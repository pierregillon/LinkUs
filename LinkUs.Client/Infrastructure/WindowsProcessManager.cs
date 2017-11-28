using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Principal;

namespace LinkUs.Client.Infrastructure
{
    public class WindowsProcessManager : IProcessManager
    {
        public bool TryStartProcessWithElevatedPrivileges(string exeFilePath)
        {
            try {
                StartProcessWithElevatedPrivileges(exeFilePath);
                return true;
            }
            catch (Win32Exception) {
                return false;
            }
        }
        public void StartProcessWithElevatedPrivileges(string exeFilePath)
        {
            var startInfo = new ProcessStartInfo(exeFilePath) {
                Verb = "runas"
            };
            Process.Start(startInfo);
        }
        public void StartProcess(string exeFilePath)
        {
            var startInfo = new ProcessStartInfo(exeFilePath);
            Process.Start(startInfo);
        }
        public bool IsProcessStarted(string fileName)
        {
            return Process.GetProcessesByName(fileName).Any();
        }
        public void StartProcessWithCurrentPrivileges(string exeFilePath)
        {
            if (IsAdministrator()) {
                StartProcessWithElevatedPrivileges(exeFilePath);
            }
            else {
                StartProcess(exeFilePath);
            }
        }

        // ----- Utils
        private static bool IsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent()) {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
    }
}