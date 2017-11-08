using System.Linq;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.Verbs;
using LinkUs.Core;
using LinkUs.Core.Connection;

namespace LinkUs.CommandLine.Handlers
{
    public class PingCommandLineHandler : ICommandLineHandler<PingCommandLine>
    {
        private readonly IConsole _console;
        private readonly RemoteClient _remoteClient;
        private readonly Server _server;

        public PingCommandLineHandler(IConsole console, ICommandSender commandSender)
        {
            _console = console;
            _remoteClient = new RemoteClient(commandSender);
            _server = new Server(commandSender);
        }

        public async Task Handle(PingCommandLine commandLine)
        {
            var clients = await _server.GetConnectedClients();
            var matchingClients = clients.Where(x => x.Id.StartsWith(commandLine.Target)).ToArray();
            if (matchingClients.Length == 0) {
                _console.WriteLineError($"The client '{commandLine.Target}' is not connected.");
            }
            else if (matchingClients.Length > 1) {
                _console.WriteLineError($"Multiple client are matching '{commandLine.Target}'.");
            }
            else {
                var targetId = ClientId.Parse(matchingClients.Single().Id);
                var pingEllapsedTime = await _remoteClient.Ping(targetId);
                _console.WriteLine($"Ok. {pingEllapsedTime} ms.");
            }
        }
    }
}