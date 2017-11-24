using System;
using System.Collections.Generic;
using LinkUs.Core.Packages;

namespace LinkUs.Modules.Default.Modules
{
    public class ExternalAssemblyModuleScanner
    {
        private readonly ExternalAssemblyModuleLocator _moduleLocator;
        private readonly PackageParser _packageParser;

        public ExternalAssemblyModuleScanner(ExternalAssemblyModuleLocator moduleLocator, PackageParser packageParser)
        {
            _moduleLocator = moduleLocator;
            _packageParser = packageParser;
        }

        public IEnumerable<ExternalAssemblyModule> Scan()
        {
            var loadedModules = new List<ExternalAssemblyModule>();
            foreach (var moduleInfo in _moduleLocator.GetModules()) {
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
                var module = new ExternalAssemblyModule(new AssemblyHandlerScanner(), _packageParser, moduleInfo.FileLocation);
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