using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class LoadModuleCommandLine : ClientTargettedCommandLine
    {
        [Option('m', "module", Required = true, HelpText = "The module name to load.")]
        public string ModuleName { get; set; }
    }
}