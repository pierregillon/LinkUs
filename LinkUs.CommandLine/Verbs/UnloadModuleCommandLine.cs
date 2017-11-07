using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class UnloadModuleCommandLine : ClientTargettedCommandLine
    {
        [Option('m', "module", Required = true, HelpText = "The module name to unload.")]
        public string ModuleName { get; set; }
    }
}