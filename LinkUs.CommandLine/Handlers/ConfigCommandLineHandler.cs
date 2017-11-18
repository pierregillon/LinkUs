using System.Threading.Tasks;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class ConfigCommandLineHandler : ICommandLineHandler<ConfigCommandLine>
    {
        private readonly GlobalParameters _parameters;

        // ----- Constructors
        public ConfigCommandLineHandler(GlobalParameters parameters)
        {
            _parameters = parameters;
        }

        // ----- Public methods
        public Task Handle(ConfigCommandLine commandLine)
        {
            if (commandLine.Any()) {
                if (string.IsNullOrEmpty(commandLine.Server) == false) {
                    _parameters.ServerHost = commandLine.Server;
                }
                if (commandLine.Port.HasValue) {
                    _parameters.ServerPort = commandLine.Port.Value;
                }
                _parameters.Save();
            }
            else {
                _parameters.Edit();
            }

            return Task.Delay(0);
        }
    }
}