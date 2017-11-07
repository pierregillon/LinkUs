using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LinkUs.Core.Connection;
using LinkUs.Core.Modules.Exceptions;

namespace LinkUs.Core.Modules
{
    public class ExternalAssemblyModule : IModule
    {
        private readonly PackageParser _packageParser;
        private readonly AppDomain _moduleDomain;
        private readonly AssemblyName _assemblyName;
        private readonly IDictionary<string, MaterializationInfo> _info = new ConcurrentDictionary<string, MaterializationInfo>();

        public string Path { get; }
        public string Name => _assemblyName.Name;

        // ----- Constructors
        public ExternalAssemblyModule(PackageParser packageParser, string filePath)
        {
            _packageParser = packageParser;

            Path = filePath;
            _assemblyName = AssemblyName.GetAssemblyName(filePath);
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
        }

        // ----- Public methods
        public void Dispose()
        {
            AppDomain.Unload(_moduleDomain);
            _info.Clear();
        }
        public object Process(string commandName, Package package, IBus bus)
        {
            MaterializationInfo materializationInfo;
            if (_info.TryGetValue(commandName, out materializationInfo) == false) {
                throw new UnknownCommandException(commandName, Name);
            }
            var handlerInstance = CreateHandlerInstance(materializationInfo.HandlerType, bus);
            var command = _packageParser.Materialize(materializationInfo.CommandType, package);
            return CallHandleMethod(handlerInstance, command);
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
        private static object CreateHandlerInstance(Type handlerType, IBus bus)
        {
            object handlerInstance;
            var handlerConstructor = handlerType.GetConstructors().First();
            var parameters = handlerConstructor.GetParameters();
            if (parameters.Length == 0) {
                handlerInstance = Activator.CreateInstance(handlerType);
            }
            else if (parameters.Length == 1) {
                if (parameters.Single().ParameterType != typeof(object)) {
                    throw new Exception($"The constructor parameter of '{handlerType.Name}' should be 'object' type.");
                }
                handlerInstance = Activator.CreateInstance(handlerType, bus);
            }
            else {
                throw new Exception($"To many parameters for the class '{handlerType.Name}'. Cannot instanciate.");
            }
            return handlerInstance;
        }
        private static object CallHandleMethod(object handlerInstance, object messageInstance)
        {
            var handle = handlerInstance
                .GetType()
                .GetMethods()
                .SingleOrDefault(x => x.Name == "Handle" && x.GetParameters()[0].ParameterType == messageInstance.GetType());

            if (handle == null) {
                throw new Exception("Unable to find the handle method.");
            }
            if (handle.ReturnType != typeof(void)) {
                return handle.Invoke(handlerInstance, new[] {messageInstance});
            }
            else {
                handle.Invoke(handlerInstance, new[] {messageInstance});
                return null;
            }
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