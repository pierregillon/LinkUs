using System;
using System.Reflection;
using LinkUs.Core;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Core.Modules;
using LinkUs.Core.Modules.Exceptions;
using LinkUs.Core.Packages;

namespace LinkUs.Client
{
    public class PackageProcessor
    {
        private readonly ICommandSender _commandSender;
        private readonly PackageParser _packageParser;
        private readonly ModuleManager _moduleManager;

        // ----- Constructors
        public PackageProcessor(
            ICommandSender commandSender,
            PackageParser packageParser,
            ModuleManager moduleManager)
        {
            _commandSender = commandSender;
            _packageParser = packageParser;
            _moduleManager = moduleManager;
        }

        // ----- Public methods
        public void Process(Package package)
        {
            try {
                var messageDescriptor = _packageParser.GetCommandDescription(package);
                var module = _moduleManager.FindModule(messageDescriptor.AssemblyName);
                var bus = new DedicatedBus(_commandSender, package);
                var response = module.Process(messageDescriptor.CommandName, package, bus);
                if (response != null) {
                    Answer(package, response);
                }
            }
            catch (UnknownCommandException ex) {
                Answer(package, new ErrorMessage(ex.Message));
            }
            catch (ModuleException ex) {
                Answer(package, new ErrorMessage(ex.Message));
            }
            catch (TargetInvocationException ex) {
                Answer(package, new ErrorMessage(ex.InnerException));
            }
            catch (Exception ex) {
                Answer(package, new ErrorMessage(ex));
            }
        }

        // ----- Internal logics
        private void Answer(Package package, object response)
        {
            _commandSender.AnswerAsync(response, package);
        }
    }
}