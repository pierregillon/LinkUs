using System;
using LinkUs.Core.Json;
using LinkUs.Core.Modules;

namespace LinkUs.Core
{
    public class Materializer
    {
        private readonly ISerializer _serializer;
        private readonly ModuleManager _moduleManager;

        public Materializer(ISerializer serializer, ModuleManager moduleManager)
        {
            _serializer = serializer;
            _moduleManager = moduleManager;
        }

        public object Materialize(byte[] bytes)
        {
            var commandName = GetCommandName(bytes);
            var commandType = _moduleManager.FindCommand(commandName);
            if (commandType == null) {
                throw new Exception("Unable to materialize command.");
            }
            return _serializer.Deserialize(bytes, commandType);
        }

        private string GetCommandName(byte[] bytes)
        {
            return _serializer.Deserialize<MessageDescriptor>(bytes).Name;
        }
    }
}