using System;
using LinkUs.Core.Json;
using LinkUs.Core.Modules;

namespace LinkUs.Core
{
    public class HandlerLocator
    {
        private readonly ModuleManager _moduleManager;

        // ----- Constructors
        public HandlerLocator(ModuleManager moduleManager)
        {
            _moduleManager = moduleManager;
        }

        // ----- Public methods
        public MaterializationInfo GetHandler(string commandName)
        {
            var handlerType = _moduleManager.FindCommandHandler(commandName);
            if (handlerType == null) {
                throw new Exception($"No handler found to process {commandName}.");
            }
            return handlerType;
        }
    }
}