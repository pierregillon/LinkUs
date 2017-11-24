using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LinkUs.Modules.Default.Modules
{
    public class ExternalAssemblyModuleLocator
    {
        private const string MODULE_DIRECTORY = "ef3cA";

        public ExternalAssemblyModuleLocator()
        {
            Directory.CreateDirectory(MODULE_DIRECTORY);
        }

        // ----- Public methods
        public IEnumerable<ModuleInformation> GetModules()
        {
            foreach (var filePath in Directory.GetFiles(MODULE_DIRECTORY)) {
                var assemblyName = AssemblyName.GetAssemblyName(filePath);
                yield return BuildModuleInformation(assemblyName, filePath);
            }
        }
        public ModuleInformation GetModuleInformation(string moduleName)
        {
            return (from filePath in Directory.GetFiles(MODULE_DIRECTORY)
                    let assemblyName = AssemblyName.GetAssemblyName(filePath)
                    where assemblyName.Name == moduleName
                    select BuildModuleInformation(assemblyName, filePath)).FirstOrDefault();
        }

        // ----- Interal logics
        private static ModuleInformation BuildModuleInformation(AssemblyName assemblyName, string filePath)
        {
            return new ModuleInformation {
                Name = assemblyName.Name,
                Version = assemblyName.Version.ToString().Split('.').Take(2).Aggregate(".", (c, s) => s + c.ToString()),
                FileLocation = filePath
            };
        }
    }
}