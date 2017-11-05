using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace LinkUs.Core.Modules
{
    public class LoadableModule : IModule
    {
        private AppDomain _moduleDomain;
        private readonly AssemblyName _assemblyName;

        public string Path { get; }
        public string Name => _assemblyName.Name;
        public Version Version => _assemblyName.Version;

        public IEnumerable<Type> AvailableHandlers { get; private set; }
        public bool IsLoaded { get; private set; }

        // ----- Constructors
        public LoadableModule(string modulepath)
        {
            try {
                Path = modulepath;
                _assemblyName = AssemblyName.GetAssemblyName(modulepath);
            }
            catch (FileNotFoundException ex) {
                throw new Exception("Module path is not valid.", ex);
            }
        }

        // ----- Public methods
        public void Load()
        {
            _moduleDomain = AppDomain.CreateDomain("ModuleDomain");

            try {
                var assembly = LoadAssembly();
                var moduleType = GetModuleClassFromAssembly(assembly);
                AvailableHandlers = GetHandlers(moduleType);
            }
            catch (Exception) {
                AppDomain.Unload(_moduleDomain);
                throw;
            }
            IsLoaded = true;
        }
        public void Unload()
        {
            AppDomain.Unload(_moduleDomain);
            IsLoaded = false;
        }

        // ----- Internal logics
        private Assembly LoadAssembly()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainOnAssemblyResolve;
            var assembly = _moduleDomain.Load(_assemblyName);
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomainOnAssemblyResolve;
            return assembly;
        }
        private static Type GetModuleClassFromAssembly(Assembly assembly)
        {
            var moduleType = assembly.GetTypes().FirstOrDefault(x => x.Name == "Module");
            if (moduleType == null) {
                throw new Exception($"The assembly '{assembly.GetName().Name}' is not a valid module. 'Module' class was not found.");
            }
            return moduleType;
        }
        private static IEnumerable<Type> GetHandlers(Type moduleType)
        {
            var module = Activator.CreateInstance(moduleType);
            var methodName = "GetHandlers";
            var method = moduleType.GetMethod(methodName);
            if (method == null) {
                throw new Exception($"Method '{methodName}' not found on the Module class.");
            }
            return (IEnumerable<Type>) method.Invoke(module, new object[0]);
        }

        // ----- Event callbacks
        private Assembly CurrentDomainOnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            if (System.IO.Path.IsPathRooted(Path)) {
                return Assembly.LoadFile(Path);
            }
            else {
                var absolutePath = System.IO.Path.Combine(Directory.GetCurrentDirectory(), Path);
                return Assembly.LoadFile(absolutePath);
            }
        }
    }
}