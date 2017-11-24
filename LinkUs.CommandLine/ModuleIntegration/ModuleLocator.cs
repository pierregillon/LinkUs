using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LinkUs.CommandLine.ModuleIntegration
{
    public class ModuleLocator
    {
        private readonly string _moduleDirectory;

        public ModuleLocator(string moduleDirectory)
        {
            _moduleDirectory = moduleDirectory;
        }

        // ----- Public methods
        public IEnumerable<Module> GetAvailableModules()
        {
            foreach (var filePath in GetAssembliesInModuleDirectory()) {
                var assemblyName = AssemblyName.GetAssemblyName(filePath);
                if (assemblyName.Name == "LinkUs.Modules.Default") {
                    continue;
                }

                yield return new Module {
                    Name = assemblyName.Name,
                    Description = GetDescription(assemblyName),
                    FileLocation = filePath
                };
            }
        }
        public string GetFullPath(string moduleName)
        {
            var fileName = moduleName + ".dll";
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
        }

        // ----- Utils
        private IEnumerable<string> GetAssembliesInModuleDirectory()
        {
            return Directory.GetFiles(_moduleDirectory, "LinkUs.Modules.*.dll");
        }
        private static string GetDescription(AssemblyName assemblyName)
        {
            var assembly = GetAssembly(assemblyName);

            if (assembly != null) {
                return assembly
                    .GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false)
                    .OfType<AssemblyDescriptionAttribute>()
                    .FirstOrDefault()
                    ?.Description;
            }

            return null;
        }
        private static Assembly GetAssembly(AssemblyName assemblyName)
        {
            var assembly = AppDomain
                .CurrentDomain
                .GetAssemblies()
                .SingleOrDefault(x => x.GetName().Name == assemblyName.Name);

            if (assembly == null) {
                assembly = Assembly.Load(assemblyName);
            }
            return assembly;
        }
    }
}