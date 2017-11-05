using System;
using System.Linq;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;

namespace LinkUs.Client
{
    public class PackageProcessor
    {
        private readonly PackageTransmitter _transmitter;
        private readonly MessageHandlerLocator _messageHandlerLocator;
        private readonly ISerializer _serializer;

        // ----- Constructors
        public PackageProcessor(PackageTransmitter transmitter, MessageHandlerLocator messageHandlerLocator, ISerializer serializer)
        {
            _transmitter = transmitter;
            _messageHandlerLocator = messageHandlerLocator;
            _serializer = serializer;
        }

        // ----- Public methods
        public void Process(Package package)
        {
            try {
                var messageName = GetMessageName(package);
                var response = ExecuteCommand(messageName, package);
                if (response != null) {
                    Answer(package, response);
                }
            }
            catch (Exception ex) {
                Answer(package, new ErrorMessage(ex.ToString()));
            }
        }

        // ----- Internal logics
        private object ExecuteCommand(string messageName, Package package)
        {
            var messageTypeName = _messageHandlerLocator.GetMessageType(messageName);
            var messageInstance = _serializer.Deserialize(package.Content, messageTypeName);
            var handlerType = _messageHandlerLocator.GetHandlerType(messageTypeName);
            var handlerInstance = CreateHandlerInstance(package, handlerType);
            return Handle(handlerInstance, messageTypeName, messageInstance);
        }
        private object CreateHandlerInstance(Package package, Type handlerType)
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
                handlerInstance = Activator.CreateInstance(handlerType, new DedicatedBus(_transmitter, package.Source, _serializer));
            }
            else {
                throw new Exception($"To many parameters for the class '{handlerType.Name}'. Cannot instanciate.");
            }
            return handlerInstance;
        }
        private static object Handle(object handlerInstance, Type messageType, object messageInstance)
        {
            var handle = handlerInstance
                .GetType()
                .GetMethods()
                .SingleOrDefault(x => x.Name == "Handle" && x.GetParameters()[0].ParameterType == messageType);

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
        private string GetMessageName(Package package)
        {
            return _serializer.Deserialize<MessageDescriptor>(package.Content).Name;
        }

        // ----- Utils
        private void Answer(Package package, object response)
        {
            var bytes = _serializer.Serialize(response);
            var responsePackage = package.CreateResponsePackage(bytes);
            _transmitter.Send(responsePackage);
        }
    }
}