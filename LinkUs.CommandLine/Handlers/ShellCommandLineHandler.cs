using System.Threading.Tasks;
using LinkUs.CommandLine.Verbs;
using LinkUs.Core.Connection;

namespace LinkUs.CommandLine.Handlers
{
    public class ShellCommandLineHandler : ICommandLineHandler<ShellCommandLine>
    {
        private readonly ConsoleRemoteShellController _remoteShellController;

        public ShellCommandLineHandler(ConsoleRemoteShellController remoteShellController)
        {
            _remoteShellController = remoteShellController;
        }

        public Task Handle(ShellCommandLine commandLine)
        {
            var targetId = ClientId.Parse(commandLine.Target);
            _remoteShellController.SendInputs(targetId);
            return Task.Delay(0);
        }
    }
}