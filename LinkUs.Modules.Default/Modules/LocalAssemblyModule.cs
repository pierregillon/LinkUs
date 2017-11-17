using System;
using System.Linq;
using System.Reflection;
using LinkUs.Core.Commands;
using LinkUs.Core.Packages;
using LinkUs.Modules.Default.Modules.Commands;
using LinkUs.Modules.Default.Modules.Exceptions;

namespace LinkUs.Modules.Default.Modules
{
    public class LocalAssemblyModule : IModule
    {
        private readonly ModuleManager _moduleManager;
        private readonly ExternalAssemblyModuleLocator _moduleLocator;
        private readonly PackageParser _packageParser;
        private readonly Type[] _assemblyTypes;

        // ----- Properties
        public string Name => GetType().Assembly.GetName().Name;

        // ----- Constructor
        public LocalAssemblyModule(
            ModuleManager moduleManager,
            ExternalAssemblyModuleLocator moduleLocator,
            PackageParser packageParser)
        {
            _moduleManager = moduleManager;
            _moduleLocator = moduleLocator;
            _packageParser = packageParser;

            _assemblyTypes = Assembly
                .GetExecutingAssembly()
                .GetTypes();
        }

        // ----- Public method
        public object Process(string commandName, Package package, IBus bus)
        {
            var commandType = _assemblyTypes.SingleOrDefault(x => x.Name == commandName);

            if (commandType == null) {
                throw new UnknownCommandException(commandName, Name);
            }

            var commandInstance = _packageParser.Materialize(commandType, package);
            if (commandInstance is ListModules) {
                return GetModuleCommandHandler().Handle((ListModules) commandInstance);
            }
            if (commandInstance is LoadModule) {
                return GetModuleCommandHandler().Handle((LoadModule) commandInstance);
            }
            if (commandInstance is UnloadModule) {
                return GetModuleCommandHandler().Handle((UnloadModule) commandInstance);
            }
            var handlerType = _assemblyTypes.SingleOrDefault(
                x => x.GetInterfaces()
                    .Any(y => y.GetGenericArguments().Length != 0 &&
                              y.GetGenericArguments()[0] == commandType));


            object handler;
            var parametersInfo = handlerType.GetConstructors()[0].GetParameters();
            if (parametersInfo.Length == 1 && parametersInfo[0].ParameterType == typeof(IBus)) {
                handler = Activator.CreateInstance(handlerType, bus);
            }
            else {
                handler = Activator.CreateInstance(handlerType);
            }

            var handle = handlerType
                .GetMethods()
                .Where(x => x.Name == "Handle")
                .Single(x => x.GetParameters()[0].ParameterType == commandType);

            return handle.Invoke(handler, new[] {commandInstance});
        }
        public void Dispose() { }

        // ----- Internal logic
        private ModuleCommandHandler GetModuleCommandHandler()
        {
            return new ModuleCommandHandler(_moduleManager, _moduleLocator, _packageParser);
        }
    }
}