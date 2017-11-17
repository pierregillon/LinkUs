using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
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
        public Task Process(string[] arguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            var commandLine = ParseArguments(arguments);
            return ExecuteCommand(commandLine);
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
                throw new InvalidCommandLineArguments(error);
            }
            return invokedVerbInstance;
        }

        // ----- Internal logic
        private Task ExecuteCommand(object commandLine)
        {
            var commandLineType = commandLine.GetType();
            var handlerContract = typeof(ICommandLineHandler<>).MakeGenericType(commandLineType);
            var handler = _container.GetInstance(handlerContract);
            var handleMethod = GetHandleMethod(handlerContract, commandLineType);
            var task = (Task)handleMethod.Invoke(handler, new[] {commandLine});
            return task;
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