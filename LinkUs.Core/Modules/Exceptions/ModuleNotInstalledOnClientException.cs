namespace LinkUs.Core.Modules.Exceptions
{
    public class ModuleNotInstalledOnClientException : ModuleException
    {
        private readonly string _moduleName;

        public ModuleNotInstalledOnClientException(string moduleName)
        {
            _moduleName = moduleName;
        }

        public override string Message => $"The module '{_moduleName}' is not installed on client.";
    }
}