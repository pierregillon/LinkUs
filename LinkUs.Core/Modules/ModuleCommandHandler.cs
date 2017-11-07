﻿using System;
using System.Linq;
using LinkUs.Core.Modules.Commands;

namespace LinkUs.Core.Modules
{
    public class ModuleCommandHandler :
        IHandler<ListModules, ModuleInformationResponse>,
        IHandler<LoadModule, bool>,
        IHandler<UnloadModule, bool>
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

        public ModuleInformationResponse Handle(ListModules command)
        {
            var externalAssemblyModules = _moduleLocator.GetModules().ToList();
            foreach (var loadedModule in _moduleManager.Modules) {
                var module = externalAssemblyModules.SingleOrDefault(x => x.Name == loadedModule.Name);
                if (module != null) {
                    module.IsLoaded = true;
                }
            }
            return new ModuleInformationResponse {
                ModuleInformations = externalAssemblyModules
            };
        }

        public bool Handle(LoadModule request)
        {
            var module = _moduleManager.GetModule(request.ModuleName);
            if (module != null) {
                throw new Exception($"Module '{request.ModuleName}' is already loaded.");
            }
            var filePath = _moduleLocator.GetFullPath(request.ModuleName);
            var externalAssemblyModule = new ExternalAssemblyModule(_packageParser, filePath);
            _moduleManager.Register(externalAssemblyModule);
            return true;
        }
        public bool Handle(UnloadModule request)
        {
            var module = _moduleManager.FindModule(request.ModuleName);
            module.Dispose();
            _moduleManager.Unregister(module);
            return true;
        }
    }
}