using System;
using System.Linq;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using LinkUs.Core.Modules;

namespace LinkUs.Client
{
    public class PackageProcessor
    {
        private readonly PackageTransmitter _transmitter;
        private readonly ISerializer _serializer;
        private readonly HandlerLocator _handlerLocator;
        private readonly PackageParser _packageParser;

        // ----- Constructors
        public PackageProcessor(
            PackageTransmitter transmitter,
            ISerializer serializer,
            HandlerLocator handlerLocator,
            PackageParser packageParser)
        {
            _transmitter = transmitter;
            _serializer = serializer;
            _handlerLocator = handlerLocator;
            _packageParser = packageParser;
        }

        // ----- Public methods
        public void Process(Package package)
        {
            try {
                var commandName = _packageParser.GetCommandName(package);
                var materializationInfo = _handlerLocator.GetHandler(commandName);
                var bus = new DedicatedBus(_transmitter, package.Source, _serializer);
                var handlerInstance = CreateHandlerInstance(materializationInfo.HandlerType, bus);
                var command = _packageParser.Materialize(materializationInfo.CommandType, package);
                var response = CallHandleMethod(handlerInstance, command);
                if (response != null) {
                    Answer(package, response);
                }
            }
            catch (Exception ex) {
                Answer(package, new ErrorMessage(ex.ToString()));
            }
        }

        // ----- Internal logics
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
        private void Answer(Package package, object response)
        {
            var bytes = _serializer.Serialize(response);
            var responsePackage = package.CreateResponsePackage(bytes);
            _transmitter.Send(responsePackage);
        }
    }
}