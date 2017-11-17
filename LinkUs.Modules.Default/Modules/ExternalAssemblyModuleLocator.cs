using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace LinkUs.Modules.Default.Modules
{
    public class ExternalAssemblyModuleLocator
    {
        private const string MODULE_DIRECTORY = ".";

        public IEnumerable<ModuleInformation> GetModules()
        {
            foreach (var filePath in Directory.GetFiles(MODULE_DIRECTORY, "LinkUs.Modules.*.dll")) {
                var assemblyName = AssemblyName.GetAssemblyName(filePath);
                if (assemblyName.Name == GetType().Assembly.GetName().Name) {
                    continue;
                }
                yield return new ModuleInformation {
                    Name = assemblyName.Name,
                    Version = assemblyName.Version.ToString()
                };
            }
        }
        public string GetFullPath(string moduleName)
        {
            if (moduleName == null) throw new ArgumentNullException(nameof(moduleName));
            var fileName = moduleName + ".dll";
            return Path.Combine(MODULE_DIRECTORY, fileName);
        }
    }
}