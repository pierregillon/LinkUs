using System;
using System.Linq;
using LinkUs.Core.Modules;
using LinkUs.Core.PingLib;

namespace LinkUs.Core
{
    public class MessageHandlerLocator
    {
        private readonly Type[] _handlerTypes;
        private readonly Type[] _types;

        public MessageHandlerLocator(IModule module)
        {
            _handlerTypes = new Type[0]
                .Union(module.AvailableHandlers)
                .Union(new[] { typeof(PingHandler) }).ToArray();

            _types = _handlerTypes
                .Select(x => x.GetMethods().Where(method => method.Name == "Handle"))
                .SelectMany(x => x)
                .Where(x => x.GetParameters().Length != 0)
                .Select(x => x.GetParameters()[0].ParameterType)
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