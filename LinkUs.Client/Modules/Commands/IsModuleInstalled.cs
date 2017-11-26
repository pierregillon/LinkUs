namespace LinkUs.Client.Modules.Commands
{
    public class IsModuleInstalled
    {
        public string ModuleName { get; set; }

        public IsModuleInstalled() { }
        public IsModuleInstalled(string moduleName)
        {
            ModuleName = moduleName;
        }
    }
}