using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;
using LinkUs.CommandLine.ConsoleLib;
using StructureMap;

namespace LinkUs.CommandLine
{
    public class CommandLineProcessor : ICommandLineProcessor
    {
        private readonly IContainer _container;
        private readonly IConsole _console;
        private readonly Parser _parser;

        // ----- Constructor
        public CommandLineProcessor(IContainer container, IConsole console, Parser parser)
        {
            _container = container;
            _console = console;
            _parser = parser;
        }

        // ----- Public methods
        public Task Process(string[] arguments)
        {
            if (arguments == null) throw new ArgumentNullException(nameof(arguments));

            object commandLine;
            if (TryParseArguments(arguments, out commandLine)) {
                return ExecuteCommand(commandLine);
            }
            return Task.Delay(0);
        }
        private bool TryParseArguments(string[] arguments, out object commandLine)
        {
            var options = new Options();
            string verbName = null;
            object commandLineIntance = null;
            if (!_parser.ParseArguments(arguments, options, (verb, subOptions) => {
                verbName = verb;
                commandLineIntance = subOptions;
            })) {
                _console.WriteLine(HelpText.AutoBuild(options, verbName));
                commandLine = null;
                return false;
            }
            else {
                commandLine = commandLineIntance;
                return true;
            }
        }

        // ----- Internal logic
        private Task ExecuteCommand(object commandLine)
        {
            var commandLineType = commandLine.GetType();
            var handlerContract = typeof(ICommandLineHandler<>).MakeGenericType(commandLineType);
            var handler = _container.GetInstance(handlerContract);
            var handleMethod = GetHandleMethod(handlerContract, commandLineType);
            var task = (Task) handleMethod.Invoke(handler, new[] { commandLine });
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