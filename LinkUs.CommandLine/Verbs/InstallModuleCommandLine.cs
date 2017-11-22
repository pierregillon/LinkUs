using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class InstallModuleCommandLine : ClientTargettedCommandLine
    {
        [Option('m', "module", Required = true, HelpText = "The module name to install.")]
        public string ModuleName { get; set; }
    }
}