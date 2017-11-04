using System;
using System.Linq;

namespace LinkUs.Core
{
    public class MessageHandlerLocator
    {
        private readonly Type[] _handlerTypes;
        private readonly Type[] _types;

        public MessageHandlerLocator()
        {
            _handlerTypes = typeof(IHandler<>)
                .Assembly
                .GetTypes()
                .Where(x => x
                    .GetInterfaces()
                    .Any(interf => interf.IsGenericType &&
                                   (interf.GetGenericTypeDefinition() == typeof(IHandler<,>) || interf.GetGenericTypeDefinition() == typeof(IHandler<>))))
                .ToArray();

            _types = typeof(IHandler<>)
                .Assembly
                .GetTypes()
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
            var messageType = _types.SingleOrDefault(x => x.Name == messageName);
            if (messageType == null) {
                throw new Exception($"Unknown message : {messageName}");
            }
            return messageType;
        }
    }
}