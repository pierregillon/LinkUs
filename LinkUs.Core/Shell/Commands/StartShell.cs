using System.Collections.Generic;

namespace LinkUs.Core.Shell.Commands
{
    public class StartShell : Message
    {
        public string CommandLine { get; set; }
        public List<object> Arguments { get; set; }
    }
}