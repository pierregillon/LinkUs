using System;
using System.Collections.Generic;
using System.IO;

namespace LinkUs.Core.Modules
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
                Console.Write($"* Loading module {moduleInfo.Name} \t ");
                var fullPath = _moduleLocator.GetFullPath(moduleInfo.Name);
                try {
                    var module = new ExternalAssemblyModule(_packageParser, fullPath);
                    loadedModules.Add(module);
                    Console.WriteLine("[OK]");
                }
                catch (Exception ex) {
                    Console.WriteLine("[FAILED] " + ex.Message);
                }
            }
            return loadedModules;
        }
    }
}