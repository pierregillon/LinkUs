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
        private readonly HandlerLocator _handlerLocator;
        private readonly ISerializer _serializer = new JsonSerializer();

        // ----- Constructors
        public PackageProcessor(PackageTransmitter transmitter, HandlerLocator handlerLocator)
        {
            _transmitter = transmitter;
            _handlerLocator = handlerLocator;
        }

        // ----- Public methods
        public void Process(Package package)
        {
            try {
                var messageName = GetMessageName(package);
                var response = ExecuteCommand(_transmitter, messageName, package);
                if (response != null) {
                    Answer(package, response);
                }
            }
            catch (Exception ex) {
                Answer(package, new ErrorMessage(ex.ToString()));
            }
        }
        private object ExecuteCommand(PackageTransmitter transmitter, string messageName, Package package)
        {
            var messageType = _handlerLocator.GetMessageType(messageName);
            if (messageType == null) {
                throw new Exception($"Unknown message : {messageName}");
            }
            var messageInstance = (Message) _serializer.Deserialize(package.Content, messageType);
            var handlerType = _handlerLocator.GetHandlerType(messageType);
            var handlerConstructor = handlerType.GetConstructors().First();
            object handlerInstance;
            if (handlerConstructor.GetParameters()[0].ParameterType == typeof(IMessageTransmitter)) {
                handlerInstance = Activator.CreateInstance(handlerType, new DedicatedMessageTransmitter(transmitter, package.Source, _serializer));
            }
            else {
                handlerInstance = Activator.CreateInstance(handlerType);
            }
            var handle = handlerInstance.GetType().GetMethods().Single(x=>x.Name == "Handle" && x.GetParameters()[0].ParameterType == messageType);
            if (handle == null) {
                throw new Exception("Unable to find the handle method.");
            }
            if (handle.ReturnType != typeof(void)) {
                return handle.Invoke(handlerInstance, new object[] {messageInstance});
            }
            else {
                handle.Invoke(handlerInstance, new object[] { messageInstance });
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

    public class HandlerLocator
    {
        private readonly Type[] _handlerTypes;
        private readonly Type[] _types;

        public HandlerLocator()
        {
            _handlerTypes = typeof(Message)
                .Assembly
                .GetTypes()
                .Where(x => x
                    .GetInterfaces()
                    .Any(interf => interf.IsGenericType &&
                               (interf.GetGenericTypeDefinition() == typeof(IHandler<,>) || interf.GetGenericTypeDefinition() == typeof(IHandler<>))))
                .ToArray();

            _types = typeof(Message)
                .Assembly
                .GetTypes()
                .Where(x => x.IsSubclassOf(typeof(Message)))
                .ToArray();
        }

        public Type GetHandlerType(Type command)
        {
            return _handlerTypes.SingleOrDefault(x => x
                .GetInterfaces()
                .Any(interf => interf.GenericTypeArguments[0] == command));
        }
        public Type GetMessageType(string messageName)
        {
            return _types.Single(x => x.Name == messageName);
        }
    }
}