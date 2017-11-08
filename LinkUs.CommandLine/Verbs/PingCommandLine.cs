using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class PingCommandLine : ClientTargettedCommandLine
    {
        [Option('c', "count", Required = false, DefaultValue = 5, HelpText = "The number of request to send.")]
        public int RequestCount { get; set; }
    }
}