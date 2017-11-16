using System.Threading.Tasks;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class ShellCommandLineHandler : ICommandLineHandler<ShellCommandLine>
    {
        private readonly ConsoleRemoteShellController _remoteShellController;
        private readonly Server _server;

        public ShellCommandLineHandler(ConsoleRemoteShellController remoteShellController, Server server)
        {
            _remoteShellController = remoteShellController;
            _server = server;
        }

        public async Task Handle(ShellCommandLine commandLine)
        {
            var client = await _server.FindRemoteClient(commandLine.Target);
            _remoteShellController.SendInputs(client.Id);
        }
    }
}