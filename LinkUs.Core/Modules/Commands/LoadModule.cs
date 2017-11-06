namespace LinkUs.Core.Modules.Commands {
    public class LoadModule
    {
        public string ModuleName { get; }

        public LoadModule(string moduleName)
        {
            ModuleName = moduleName;
        }
    }
}