namespace LinkUs.Core.Modules.Commands
{
    public class UnloadModule
    {
        public string ModuleName { get; set; }

        public UnloadModule() { }
        public UnloadModule(string moduleName)
        {
            ModuleName = moduleName;
        }
    }
}
