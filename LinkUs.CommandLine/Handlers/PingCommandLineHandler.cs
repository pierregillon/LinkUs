using System.Threading;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.Verbs;
using LinkUs.Core.Connection;

namespace LinkUs.CommandLine.Handlers
{
    public class PingCommandLineHandler : ICommandLineHandler<PingCommandLine>
    {
        private readonly IConsole _console;
        private readonly RemoteClient _client;
        private readonly Server _server;

        public PingCommandLineHandler(IConsole console, RemoteClient client, Server server)
        {
            _console = console;
            _client = client;
            _server = server;
        }

        public async Task Handle(PingCommandLine commandLine)
        {
            var client = await _server.GetConnectedClient(commandLine.Target);
            var clientId = ClientId.Parse(client.Id);
            _console.NewLine();
            for (int requestNumber = 1; requestNumber <= commandLine.RequestCount; requestNumber++) {
                _console.Write($"* {requestNumber}. Send ping request to {client.MachineName} ... ");
                var pingEllapsedTime = await _client.Ping(clientId);
                _console.WriteLine($"[OK] {pingEllapsedTime}ms.");
                if (requestNumber != commandLine.RequestCount) {
                    Thread.Sleep(1000);
                }
            }
            _console.NewLine();
        }
    }
}