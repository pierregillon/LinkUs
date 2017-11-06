using System;
using System.Collections.Generic;
using System.IO;

namespace LinkUs.Core.Modules
{
    public class ModuleAssemblyScanner
    {
        private readonly PackageParser _packageParser;
        private const string MODULE_DIRECTORY = ".";

        public ModuleAssemblyScanner(PackageParser packageParser)
        {
            _packageParser = packageParser;
        }

        public IEnumerable<IModule> Scan(string directory)
        {
            if (directory == null) throw new ArgumentNullException(nameof(directory));
            if (Directory.Exists(directory) == false) {
                throw new Exception($"Unable to load module from {directory}: the directory was not found.");
            }
            var loadedModules = new List<IModule>();
            foreach (var filePath in Directory.GetFiles(MODULE_DIRECTORY, "LinkUs.Modules.*.dll")) {
                Console.Write($"* Loading module {Path.GetFileName(filePath)} \t ");
                try {
                    var module = new ExternalAssemblyModule(_packageParser, filePath);
                    module.Load();
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