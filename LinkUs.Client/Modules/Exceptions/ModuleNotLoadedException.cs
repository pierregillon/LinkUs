namespace LinkUs.Client.Modules.Exceptions
{
    public class ModuleNotLoadedException : ModuleException
    {
        private readonly string _moduleName;

        public ModuleNotLoadedException(string moduleName)
        {
            _moduleName = moduleName;
        }

        public override string Message => $"The module '{_moduleName}' is not loaded.";
    }
}