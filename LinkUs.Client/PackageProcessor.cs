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
        private readonly PackageParser _packageParser;
        private readonly ModuleManager _moduleManager;

        // ----- Constructors
        public PackageProcessor(
            PackageTransmitter transmitter,
            ISerializer serializer,
            PackageParser packageParser,
            ModuleManager moduleManager)
        {
            _transmitter = transmitter;
            _serializer = serializer;
            _packageParser = packageParser;
            _moduleManager = moduleManager;
        }

        // ----- Public methods
        public void Process(Package package)
        {
            try {
                var messageDescriptor = _packageParser.GetCommandDescription(package);
                var module = _moduleManager.GetModule(messageDescriptor.AssemblyName);
                if (module == null) {
                    throw new Exception($"Unable to process the command '{messageDescriptor.Name}': the module '{messageDescriptor.AssemblyName}' is not loaded.");
                }
                var bus = new DedicatedBus(_transmitter, package.Source, _serializer);
                var response = module.Process(messageDescriptor.Name, package, bus);
                if (response != null) {
                    Answer(package, response);
                }
            }
            catch (Exception ex) {
                Answer(package, new ErrorMessage(ex.ToString()));
            }
        }

        // ----- Internal logics
        private void Answer(Package package, object response)
        {
            var bytes = _serializer.Serialize(response);
            var responsePackage = package.CreateResponsePackage(bytes);
            _transmitter.Send(responsePackage);
        }
    }
}