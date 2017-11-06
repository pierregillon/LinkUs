using System;
using LinkUs.Core.Modules.Commands;

namespace LinkUs.Core.Modules
{
    public class ModuleCommandHandler :
        IHandler<ListModules, ModuleInformationResponse>,
        IHandler<LoadModule, bool>
    {
        private readonly ModuleManager _moduleManager;

        public ModuleCommandHandler(ModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
        }

        public ModuleInformationResponse Handle(ListModules command)
        {
            var response = new ModuleInformationResponse();
            foreach (var module in _moduleManager.Modules) {
                response.ModuleInformations.Add(module.GetStatus());
            }
            return response;
        }

        public bool Handle(LoadModule request)
        {
            var module = _moduleManager.GetModule(request.ModuleName);
            if (module == null) {
                throw new Exception($"Module '{request.ModuleName}' was not found.");
            }
            if (module is LocalAssemblyModule) {
                throw new Exception($"Cannot load/unload with default module '{request.ModuleName}'.");
            }
            if (module is ExternalAssemblyModule == false) {
                throw new NotImplementedException("Module implementation is invalid.");
            }
            var externalAssemblyModule = (ExternalAssemblyModule) module;
            externalAssemblyModule.Load();
            return true;
        }
    }
}