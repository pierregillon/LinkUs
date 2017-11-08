using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.Verbs;

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
            var targetId = await _server.FindCliendId(commandLine.Target);
            var pingEllapsedTime = await _client.Ping(targetId);
            _console.WriteLine($"Ok. {pingEllapsedTime} ms.");
        }
    }
}