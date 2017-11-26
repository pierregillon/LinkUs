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
        public IEnumerable<ModuleInformation> GetModuleInformations()
        {
            foreach (var filePath in Directory.GetFiles(MODULE_DIRECTORY)) {
                AssemblyName assemblyName;
                try {
                    assemblyName = AssemblyName.GetAssemblyName(filePath);
                }
                catch (BadImageFormatException) {
                    continue;
                }
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
        public string GetModulesLocation()
        {
            return MODULE_DIRECTORY;
        }

        // ----- Interal logics
        private static ModuleInformation BuildModuleInformation(AssemblyName assemblyName, string filePath)
        {
            return new ModuleInformation {
                Name = assemblyName.Name,
                FileLocation = filePath
            };
        }
    }
}