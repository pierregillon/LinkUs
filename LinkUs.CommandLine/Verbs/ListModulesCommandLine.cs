using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class ListModulesCommandLine
    {
        [Option('a', "all", 
            DefaultValue = false, 
            Required = false, 
            HelpText = "Display all the available modules.",
            MutuallyExclusiveSet = "target")]
        public bool ListAvailableModules { get; set; }


        [Option('t', "target", 
            Required = false, 
            HelpText = "The remote client to list all the modules installed.")]
        public string Target { get; set; }
    }
}