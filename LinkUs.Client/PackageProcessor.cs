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
            var messageType = _messageHandlerLocator.GetMessageType(messageName);
            var messageInstance = (Message) _serializer.Deserialize(package.Content, messageType);
            var handlerType = _messageHandlerLocator.GetHandlerType(messageType);
            var handlerInstance = CreateHandlerInstance(package, handlerType);
            return Handle(handlerInstance, messageType, messageInstance);
        }
        private object CreateHandlerInstance(Package package, Type handlerType)
        {
            object handlerInstance;
            var handlerConstructor = handlerType.GetConstructors().First();
            if (handlerConstructor.GetParameters()[0].ParameterType == typeof(IMessageTransmitter)) {
                handlerInstance = Activator.CreateInstance(handlerType, new DedicatedMessageTransmitter(_transmitter, package.Source, _serializer));
            }
            else {
                handlerInstance = Activator.CreateInstance(handlerType);
            }
            return handlerInstance;
        }
        private static object Handle(object handlerInstance, Type messageType, Message messageInstance)
        {
            var handle = handlerInstance
                .GetType()
                .GetMethods()
                .SingleOrDefault(x => x.Name == "Handle" && x.GetParameters()[0].ParameterType == messageType);

            if (handle == null) {
                throw new Exception("Unable to find the handle method.");
            }
            if (handle.ReturnType != typeof(void)) {
                return handle.Invoke(handlerInstance, new object[] {messageInstance});
            }
            else {
                handle.Invoke(handlerInstance, new object[] {messageInstance});
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