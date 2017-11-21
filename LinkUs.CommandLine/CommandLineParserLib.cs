using System.Linq;
using CommandLine;
using CommandLine.Text;

namespace LinkUs.CommandLine
{
    public class CommandLineParserLib : ICommandLineParser
    {
        private readonly Parser _parser;

        public CommandLineParserLib(Parser parser)
        {
            _parser = parser;
        }

        public object Parse(string[] arguments)
        {
            var options = new Options();

            if (arguments.Any() == false) {
                throw new ArgumentParseException(HelpText.AutoBuild(options));
            }

            string verbName = null;
            object commandLineIntance = null;

            if (!_parser.ParseArguments(arguments, options, (verb, subOptions) => {
                verbName = verb;
                commandLineIntance = subOptions;
            })) {
                throw new ArgumentParseException(HelpText.AutoBuild(options, verbName));
            }
            else {
                return commandLineIntance;
            }
        }
    }
}