using System.Collections.Generic;

namespace LinkUs.Core
{
    public class ExecuteRemoteCommandLine : Command
    {
        public string CommandLine { get; set; }
        public List<object> Arguments { get; set; }
    }
}