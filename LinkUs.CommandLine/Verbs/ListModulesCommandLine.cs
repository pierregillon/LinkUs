using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class ListModulesCommandLine
    {
        [Option('t', "target", HelpText = "The remote client to list all the modules installed.")]
        public string Target { get; set; }
    }
}