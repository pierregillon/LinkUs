using System;
using System.Collections.Generic;
using System.Linq;
using LinkUs.Core.Json;
using LinkUs.Core.Modules.Exceptions;

namespace LinkUs.Core.Modules
{
    public class ModuleManager
    {
        private readonly PackageParser _packageParser;
        private readonly ExternalAssemblyModuleLocator _moduleLocator;
        private readonly ExternalAssemblyModuleScanner _moduleScanner;
        private readonly List<IModule> _modules = new List<IModule>();

        public IEnumerable<IModule> Modules => _modules;

        public ModuleManager(
            PackageParser packageParser,
            ExternalAssemblyModuleLocator moduleLocator,
            ExternalAssemblyModuleScanner moduleScanner)
        {
            _packageParser = packageParser;
            _moduleLocator = moduleLocator;
            _moduleScanner = moduleScanner;
        }

        public void Register(IModule module)
        {
            _modules.Add(module);
        }
        public void Unregister(IModule module)
        {
            _modules.Remove(module);
        }
        public IModule FindModule(string moduleName)
        {
            var module = GetModule(moduleName);
            if (module == null) {
                throw new ModuleNotLoadedException(moduleName);
            }
            return module;
        }
        public IModule GetModule(string moduleName)
        {
            return _modules.SingleOrDefault(x => string.Equals(x.Name, moduleName, StringComparison.CurrentCultureIgnoreCase));
        }
        public void LoadModules()
        {
            Register(new LocalAssemblyModule(this, _moduleLocator, _packageParser));

            foreach (var module in _moduleScanner.Scan()) {
                Register(module);
            }
        }
    }
}