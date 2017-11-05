using System;
using System.Collections.Concurrent;
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
        private bool _isLoaded;
        private readonly IDictionary<string, MaterializationInfo> _info = new ConcurrentDictionary<string, MaterializationInfo>();

        public string Path { get; }

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
                var handlers = GetHandlers(moduleType);
                foreach (var handler in handlers) {
                    foreach (var methodInfo in handler.GetMethods().Where(x => x.Name == "Handle")) {
                        var parameterType = methodInfo.GetParameters()[0].ParameterType;
                        _info.Add(parameterType.Name, new MaterializationInfo {
                            CommandType = parameterType,
                            HandlerType = handler
                        });
                    }
                }
            }
            catch (Exception) {
                AppDomain.Unload(_moduleDomain);
                throw;
            }
            _isLoaded = true;
        }
        public void Unload()
        {
            AppDomain.Unload(_moduleDomain);
            _isLoaded = false;
        }
        public ModuleInformation GetStatus()
        {
            return new ModuleInformation {
                Name = _assemblyName.Name,
                IsLoaded = _isLoaded,
                Version = _assemblyName.Version.ToString()
            };
        }
        public MaterializationInfo GetMaterializationInfo(string commandName)
        {
            MaterializationInfo info;
            if (_info.TryGetValue(commandName, out info)) {
                return info;
            }
            throw new Exception("Cannot materialize.");
        }
        public bool CanProcess(string commandName)
        {
            return _info.ContainsKey(commandName);
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