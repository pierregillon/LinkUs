using System.Threading.Tasks;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class ShellCommandLineHandler : PartialClientIdHandler, ICommandLineHandler<ShellCommandLine>
    {
        private readonly ConsoleRemoteShellController _remoteShellController;

        public ShellCommandLineHandler(ConsoleRemoteShellController remoteShellController, Server server) : base(server)
        {
            _remoteShellController = remoteShellController;
        }

        public async Task Handle(ShellCommandLine commandLine)
        {
            var targetId = await FindCliendId(commandLine.Target);
            _remoteShellController.SendInputs(targetId);
        }
    }
}