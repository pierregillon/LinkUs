using System;
using System.Linq;
using System.Reflection;
using CommandLine;
using CommandLine.Text;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.Core;
using StructureMap;

namespace LinkUs.CommandLine
{
    public class CommandLineProcessor : ICommandLineProcessor
    {
        private readonly IContainer _container;
        private readonly Parser _parser;

        // ----- Constructor
        public CommandLineProcessor(IContainer container, Parser parser)
        {
            _container = container;
            _parser = parser;
        }

        // ----- Public methods
        public void Process(string[] arguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            var commandLine = ParseArguments(arguments);
            ExecuteCommand(commandLine);
        }
        private object ParseArguments(string[] arguments)
        {
            var options = new Options();
            string invokedVerb = null;
            object invokedVerbInstance = null;
            if (!_parser.ParseArguments(arguments, options, (verb, subOptions) => {
                invokedVerb = verb;
                invokedVerbInstance = subOptions;
            })) {
                var error = HelpText.AutoBuild(options, invokedVerb);
                throw new CommandLineProcessingFailed(error);
            }
            return invokedVerbInstance;
        }

        // ----- Internal logic
        private void ExecuteCommand(object commandLine)
        {
            var commandLineType = commandLine.GetType();
            var handlerContract = typeof(IHandler<>).MakeGenericType(commandLineType);
            var handler = _container.GetInstance(handlerContract);
            var handleMethod = GetHandleMethod(handlerContract, commandLineType);
            handleMethod.Invoke(handler, new[] {commandLine});
        }
        private static MethodInfo GetHandleMethod(Type handlerType, Type commandLineType)
        {
            return handlerType
                .GetMethods()
                .Where(x => x.Name == "Handle")
                .Single(x => x.GetParameters()[0].ParameterType == commandLineType);
        }
    }
}