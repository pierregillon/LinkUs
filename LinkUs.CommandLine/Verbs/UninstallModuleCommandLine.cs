using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class UninstallModuleCommandLine : ClientTargettedCommandLine
    {
        [Option('m', "module", Required = true, HelpText = "The module name to uninstall.")]
        public string ModuleName { get; set; }
    }
}