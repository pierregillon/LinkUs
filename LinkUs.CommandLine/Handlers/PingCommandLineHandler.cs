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
        private readonly Server _server;

        public PingCommandLineHandler(IConsole console, Server server)
        {
            _console = console;
            _server = server;
        }

        public async Task Handle(PingCommandLine commandLine)
        {
            var client = await _server.FindRemoteClient(commandLine.Target);
            _console.NewLine();
            for (var requestNumber = 1; requestNumber <= commandLine.RequestCount; requestNumber++) {
                _console.Write($"* {requestNumber}. Send ping request to {client.Information.MachineName} ... ");
                var pingEllapsedTime = await client.Ping();
                _console.WriteLine($"[OK] {pingEllapsedTime}ms.");
                if (requestNumber != commandLine.RequestCount) {
                    Thread.Sleep(1000);
                }
            }
        }
    }
}