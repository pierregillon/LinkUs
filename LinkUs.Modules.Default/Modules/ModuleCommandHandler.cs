using System.Linq;
using LinkUs.Core.Commands;
using LinkUs.Core.Packages;
using LinkUs.Modules.Default.Modules.Commands;
using LinkUs.Modules.Default.Modules.Exceptions;

namespace LinkUs.Modules.Default.Modules
{
    public class ModuleCommandHandler :
        ICommandHandler<ListModules, ModuleInformation[]>,
        ICommandHandler<LoadModule, bool>,
        ICommandHandler<UnloadModule, bool>,
        ICommandHandler<IsModuleInstalled, bool>
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

        // ----- Public methods
        public ModuleInformation[] Handle(ListModules command)
        {
            return _moduleManager.Modules.Select(x => new ModuleInformation {
                Name = x.Name,
                FileLocation = x.FileLocation
            }).ToArray();
        }
        public bool Handle(LoadModule command)
        {
            var module = _moduleManager.GetModule(command.ModuleName);
            if (module != null) {
                throw new ModuleAlreadyLoadedException(command.ModuleName);
            }
            var moduleInformation = _moduleLocator.GetModuleInformation(command.ModuleName);
            if (moduleInformation == null) {
                throw new ModuleNotInstalledOnClientException(command.ModuleName);
            }
            var externalAssemblyModule = new ExternalAssemblyModule(new AssemblyHandlerScanner(), _packageParser, moduleInformation.FileLocation);
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
        public bool Handle(IsModuleInstalled command)
        {
            var module = _moduleManager.GetModule(command.ModuleName);
            return module != null;
        }
    }
}