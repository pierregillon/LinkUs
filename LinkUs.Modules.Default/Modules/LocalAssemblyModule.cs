using System;
using System.Linq;
using System.Reflection;
using LinkUs.Core;
using LinkUs.Core.Commands;
using LinkUs.Core.Packages;
using LinkUs.Modules.Default.Modules.Commands;
using LinkUs.Modules.Default.Modules.Exceptions;

namespace LinkUs.Modules.Default.Modules
{
    public class LocalAssemblyModule : IModule
    {
        private readonly PackageParser _packageParser;
        private readonly Ioc _ioc;
        private readonly Type[] _assemblyTypes;

        // ----- Properties
        public string Name => "LinkUs.Modules.Default";

        // ----- Constructor
        public LocalAssemblyModule(PackageParser packageParser, Ioc ioc)
        {
            _packageParser = packageParser;
            _ioc = ioc;

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

            var handlerType = _assemblyTypes.SingleOrDefault(
                x => x.GetInterfaces()
                      .Any(y => y.GetGenericArguments().Length != 0 &&
                                y.GetGenericArguments()[0] == commandType));

            var handler = _ioc.GetInstance(handlerType);

            var handle = handlerType
                .GetMethods()
                .Where(x => x.Name == "Handle")
                .Single(x => x.GetParameters()[0].ParameterType == commandType);

            return handle.Invoke(handler, new[] { commandInstance });
        }
        public void Dispose() { }
    }
}