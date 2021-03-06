using System;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;

namespace LinkUs.Core
{
    public class PackageParser
    {
        private readonly ISerializer _serializer;

        public PackageParser(ISerializer serializer)
        {
            _serializer = serializer;
        }

        public MessageDescriptor GetCommandDescription(Package package)
        {
            return _serializer.Deserialize<MessageDescriptor>(package.Content);
        }

        public object Materialize(Type type, Package package)
        {
            return _serializer.Deserialize(package.Content, type);
        }
    }
}