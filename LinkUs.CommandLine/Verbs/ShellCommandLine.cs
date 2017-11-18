using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class ShellCommandLine : ClientTargettedCommandLine
    {
        [Option('c', "command", Required = false, HelpText = "The shell command to execute remotely.")]
        public string Command{ get; set; }
    }
}