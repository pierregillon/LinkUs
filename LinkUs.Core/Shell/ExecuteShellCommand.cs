using System.Collections.Generic;

namespace LinkUs.Core.Shell
{
    public class ExecuteShellCommand : Command
    {
        public ExecuteShellCommand() : base("ExecuteShellCommand") { }

        public string CommandLine { get; set; }
        public List<object> Arguments { get; set; }
    }
}