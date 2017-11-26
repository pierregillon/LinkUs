using System;
using System.Collections.Generic;
using System.Linq;

namespace LinkUs.Client.Modules
{
    public class ExternalAssemblyModuleScanner
    {
        private readonly ExternalAssemblyModuleLocator _moduleLocator;
        private readonly IModuleFactory<ExternalAssemblyModule> _externalAssemblyModuleFactory;

        public ExternalAssemblyModuleScanner(
            ExternalAssemblyModuleLocator moduleLocator,
            IModuleFactory<ExternalAssemblyModule> externalAssemblyModuleFactory)
        {
            _moduleLocator = moduleLocator;
            _externalAssemblyModuleFactory = externalAssemblyModuleFactory;
        }

        public IEnumerable<ExternalAssemblyModule> Scan()
        {
            var loadedModules = new List<ExternalAssemblyModule>();
            foreach (var moduleInfo in _moduleLocator.GetModuleInformations()) {
                if (loadedModules.Any(x => x.Name == moduleInfo.Name)) {
                    continue;
                }
                var module = LoadModule(moduleInfo);
                if (module != null) {
                    loadedModules.Add(module);
                }
            }
            return loadedModules;
        }

        // ----- Internal logic
        private ExternalAssemblyModule LoadModule(ModuleInformation moduleInfo)
        {
            try {
                Console.Write($"* Loading module {moduleInfo.Name} \t ");
                var module = _externalAssemblyModuleFactory.Build(moduleInfo.FileLocation);
                Console.WriteLine("[OK]");
                return module;
            }
            catch (Exception ex) {
                Console.WriteLine("[FAILED] " + ex.Message);
                return null;
            }
        }
    }
}