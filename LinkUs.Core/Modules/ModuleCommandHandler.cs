using System.IO;
using System.Linq;
using LinkUs.Core.Commands;
using LinkUs.Core.Modules.Commands;
using LinkUs.Core.Modules.Exceptions;
using LinkUs.Core.Packages;

namespace LinkUs.Core.Modules
{
    public class ModuleCommandHandler :
        ICommandHandler<ListModules, ModuleInformation[]>,
        ICommandHandler<LoadModule, bool>,
        ICommandHandler<UnloadModule, bool>
    {
        private readonly ModuleManager _moduleManager;
        private readonly ExternalAssemblyModuleLocator _moduleLocator;
        private readonly PackageParser _packageParser;

        public ModuleCommandHandler(
            ModuleManager moduleManager,
            ExternalAssemblyModuleLocator moduleLocator,
            PackageParser packageParser)
        {
            _moduleManager = moduleManager;
            _moduleLocator = moduleLocator;
            _packageParser = packageParser;
        }

        public ModuleInformation[] Handle(ListModules command)
        {
            var externalAssemblyModules = _moduleLocator.GetModules().ToList();
            foreach (var loadedModule in _moduleManager.Modules) {
                var module = externalAssemblyModules.SingleOrDefault(x => x.Name == loadedModule.Name);
                if (module != null) {
                    module.IsLoaded = true;
                }
            }
            return externalAssemblyModules.ToArray();
        }

        public bool Handle(LoadModule command)
        {
            var module = _moduleManager.GetModule(command.ModuleName);
            if (module != null) {
                throw new ModuleAlreadyLoadedException(command.ModuleName);
            }
            var filePath = _moduleLocator.GetFullPath(command.ModuleName);
            if (File.Exists(filePath) == false) {
                throw new ModuleNotInstalledOnClientException(command.ModuleName);
            }
            var externalAssemblyModule = new ExternalAssemblyModule(new AssemblyHandlerScanner(), _packageParser, filePath);
            _moduleManager.Register(externalAssemblyModule);
            return true;
        }

        public bool Handle(UnloadModule command)
        {
            var module = _moduleManager.FindModule(command.ModuleName);
            module.Dispose();
            _moduleManager.Unregister(module);
            return true;
        }
    }
}