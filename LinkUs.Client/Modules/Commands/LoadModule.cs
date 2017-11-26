namespace LinkUs.Client.Modules.Commands
{
    public class LoadModule
    {
        public string ModuleName { get; set; }

        public LoadModule() { }
        public LoadModule(string moduleName)
        {
            ModuleName = moduleName;
        }
    }
}