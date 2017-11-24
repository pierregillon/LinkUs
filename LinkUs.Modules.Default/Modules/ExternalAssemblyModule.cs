using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using LinkUs.Core.Commands;
using LinkUs.Core.Packages;
using LinkUs.Modules.Default.Modules.Exceptions;

namespace LinkUs.Modules.Default.Modules
{
    public class ExternalAssemblyModule : IModule
    {
        private readonly PackageParser _packageParser;
        private readonly AppDomain _moduleDomain;
        private readonly AssemblyName _assemblyName;
        private readonly IDictionary<string, MaterializationInfo> _materializationInfos;

        public string Name => _assemblyName.Name;
        public string FileLocation { get; }

        // ----- Constructors
        public ExternalAssemblyModule(
            AssemblyHandlerScanner assemblyHandlerScanner, 
            PackageParser packageParser, 
            string filePath)
        {
            _packageParser = packageParser;
            FileLocation = filePath;

            _assemblyName = AssemblyName.GetAssemblyName(filePath);
            _moduleDomain = AppDomain.CreateDomain("ModuleDomain");

            try {
                var assembly = _moduleDomain.Load(File.ReadAllBytes(filePath));
                _materializationInfos = assemblyHandlerScanner.Scan(assembly);
            }
            catch (Exception) {
                AppDomain.Unload(_moduleDomain);
                throw;
            }
        }

        // ----- Public methods
        public object Process(string commandName, Package package, IBus bus)
        {
            MaterializationInfo materializationInfo;
            if (_materializationInfos.TryGetValue(commandName, out materializationInfo) == false) {
                throw new UnknownCommandException(commandName, Name);
            }

            var handlerInstance = CreateHandler(materializationInfo, bus);
            var command = _packageParser.Materialize(materializationInfo.CommandType, package);
            return materializationInfo.HandleMethod.Invoke(handlerInstance, new[] {command});
        }
        public void Dispose()
        {
            AppDomain.Unload(_moduleDomain);
            _materializationInfos.Clear();
        }

        // ----- Internal logics
        public object CreateHandler(MaterializationInfo materializationInfo, IBus bus)
        {
            object handlerInstance;
            var handlerConstructor = materializationInfo.HandlerType.GetConstructors().First();
            var parameters = handlerConstructor.GetParameters();
            if (parameters.Length == 0) {
                handlerInstance = Activator.CreateInstance(materializationInfo.HandlerType);
            }
            else if (parameters.Length == 1) {
                if (parameters.Single().ParameterType != typeof(object)) {
                    throw new Exception($"The constructor parameter of '{materializationInfo.HandlerType.Name}' should be 'object' type.");
                }
                handlerInstance = Activator.CreateInstance(materializationInfo.HandlerType, bus);
            }
            else {
                throw new Exception($"To many parameters for the class '{materializationInfo.HandlerType.Name}'. Cannot instanciate.");
            }
            return handlerInstance;
        }
    }
}