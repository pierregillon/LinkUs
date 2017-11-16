using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public abstract class ClientTargettedCommandLine
    {
        [Option('t', "target", Required = true, HelpText = "The target machine.")]
        public string Target { get; set; }
    }
}