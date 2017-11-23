using System.Threading.Tasks;
using LinkUs.CommandLine.ModuleIntegration.RemoteShell;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class ShellCommandLineHandler : ICommandLineHandler<ShellCommandLine>
    {
        private readonly ConsoleRemoteShellController _remoteShellController;
        private readonly ModuleIntegration.Default.RemoteServer _server;

        public ShellCommandLineHandler(ConsoleRemoteShellController remoteShellController, ModuleIntegration.Default.RemoteServer server)
        {
            _remoteShellController = remoteShellController;
            _server = server;
        }

        public async Task Handle(ShellCommandLine commandLine)
        {
            var client = await _server.FindRemoteClient(commandLine.Target);
            _remoteShellController.ProcessRemoteShellSession(client.TargetId, commandLine.Command);
        }
    }
}