using System;
using System.Collections.Generic;
using System.Linq;
using LinkUs.Client.Modules.Exceptions;
using LinkUs.Core;
using LinkUs.Core.Packages;

namespace LinkUs.Client.Modules
{
    public class ModuleManager
    {
        private readonly PackageParser _packageParser;
        private readonly ExternalAssemblyModuleScanner _moduleScanner;
        private readonly Ioc _ioc;
        private readonly List<IModule> _modules = new List<IModule>();

        public IEnumerable<IModule> Modules => _modules;

        public ModuleManager(
            PackageParser packageParser,
            ExternalAssemblyModuleScanner moduleScanner,
            Ioc ioc)
        {
            _packageParser = packageParser;
            _moduleScanner = moduleScanner;
            _ioc = ioc;
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
            Register(new LocalAssemblyModule(_packageParser, _ioc));

            foreach (var module in _moduleScanner.Scan()) {
                Register(module);
            }
        }
    }
}