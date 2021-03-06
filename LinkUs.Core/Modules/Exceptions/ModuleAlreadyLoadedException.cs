namespace LinkUs.Core.Modules.Exceptions
{
    public class ModuleAlreadyLoadedException : ModuleException
    {
        private readonly string _moduleName;

        public ModuleAlreadyLoadedException(string moduleName)
        {
            _moduleName = moduleName;
        }

        public override string Message => $"The module '{_moduleName}' is already loaded.";
    }
}