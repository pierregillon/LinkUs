using LinkUs.CommandLine.Verbs;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;

namespace LinkUs.CommandLine.Handlers
{
    public class ShellCommandLineHandler:IHandler<ShellCommandLine>
    {
        private readonly CommandDispatcher _commandDispatcher;

        public ShellCommandLineHandler(CommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        public void Handle(ShellCommandLine command)
        {
            var targetId = ClientId.Parse(command.Target);
            var driver = new ConsoleRemoteShellController(_commandDispatcher, targetId, new JsonSerializer());
            driver.SendInputs();
        }
    }
}
