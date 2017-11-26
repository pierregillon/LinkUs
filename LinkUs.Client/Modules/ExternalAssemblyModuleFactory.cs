using LinkUs.Core.Packages;

namespace LinkUs.Client.Modules
{
    public class ExternalAssemblyModuleFactory : IModuleFactory<ExternalAssemblyModule>
    {
        private readonly AssemblyHandlerScanner _handlerScanner;
        private readonly PackageParser _packageParser;

        public ExternalAssemblyModuleFactory(
            AssemblyHandlerScanner handlerScanner,
            PackageParser packageParser)
        {
            _handlerScanner = handlerScanner;
            _packageParser = packageParser;
        }

        public ExternalAssemblyModule Build(string fileLocation)
        {
            return new ExternalAssemblyModule(_handlerScanner, _packageParser, fileLocation);
        }
    }
}