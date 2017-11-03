using System.Collections.Generic;

namespace LinkUs.Core.Shell.Commands
{
    public class StartShellCommand : Message
    {
        public string CommandLine { get; set; }
        public List<object> Arguments { get; set; }
    }
}