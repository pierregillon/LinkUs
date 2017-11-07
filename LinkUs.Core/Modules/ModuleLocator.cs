using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using LinkUs.Core.Modules.Commands;

namespace LinkUs.Core.Modules
{
    public class ModuleLocator
    {
        private const string MODULE_DIRECTORY = ".";

        public IEnumerable<ModuleInformation> GetAllExternalAssemblyModules()
        {
            foreach (var filePath in Directory.GetFiles(MODULE_DIRECTORY, "LinkUs.Modules.*.dll")) {
                var assemblyName = AssemblyName.GetAssemblyName(filePath);
                yield return new ModuleInformation {
                    Name = assemblyName.Name,
                    Version = assemblyName.Version.ToString()
                };
            }
        }
        public string GetFullPath(string moduleName)
        {
            if (moduleName == null) throw new ArgumentNullException(nameof(moduleName));
            var fileName = Path.ChangeExtension(moduleName, ".dll");
            return Path.Combine(MODULE_DIRECTORY, fileName);
        }
    }
}