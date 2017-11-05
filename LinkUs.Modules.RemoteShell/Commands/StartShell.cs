using System.Collections.Generic;

namespace LinkUs.Modules.RemoteShell.Commands
{
    public class StartShell 
    {
        public string CommandLine { get; set; }
        public List<object> Arguments { get; set; }
    }
}