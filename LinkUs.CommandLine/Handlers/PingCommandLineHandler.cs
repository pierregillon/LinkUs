using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.Verbs;
using LinkUs.Core;

namespace LinkUs.CommandLine.Handlers
{
    public class PingCommandLineHandler : PartialClientIdHandler, ICommandLineHandler<PingCommandLine>
    {
        private readonly IConsole _console;
        private readonly RemoteClient _client;

        public PingCommandLineHandler(IConsole console, RemoteClient client, Server server) : base(server)
        {
            _console = console;
            _client = client;
        }

        public async Task Handle(PingCommandLine commandLine)
        {
            var targetId = await FindCliendId(commandLine.Target);
            var pingEllapsedTime = await _client.Ping(targetId);
            _console.WriteLine($"Ok. {pingEllapsedTime} ms.");
        }
    }
}