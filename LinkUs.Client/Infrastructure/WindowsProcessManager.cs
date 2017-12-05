using System.Diagnostics;
using System.Linq;

namespace LinkUs.Client.Infrastructure
{
    public class WindowsProcessManager : IProcessManager
    {
        public void StartProcess(string exeFilePath)
        {
            var startInfo = new ProcessStartInfo(exeFilePath);
            Process.Start(startInfo);
        }
        public bool IsProcessStarted(string fileName)
        {
            return Process.GetProcessesByName(fileName).Any();
        }
    }
}