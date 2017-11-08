using System;
using System.Linq;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.Verbs;
using LinkUs.Core;
using LinkUs.Core.Connection;

namespace LinkUs.CommandLine.Handlers
{
    public class PingCommandLineHandler : IHandler<PingCommandLine>
    {
        private readonly IConsole _console;
        private readonly RemoteClient _remoteClient;
        private readonly Server _server;

        public PingCommandLineHandler(IConsole console, CommandDispatcher commandDispatcher)
        {
            _console = console;
            _remoteClient = new RemoteClient(commandDispatcher);
            _server = new Server(commandDispatcher);
        }

        public void Handle(PingCommandLine commandLine)
        {
            var clients = _server.GetConnectedClients();
            var matchingClients = clients.Where(x => x.Id.StartsWith(commandLine.Target)).ToArray();
            if (matchingClients.Length == 0) {
                _console.WriteLineError($"The client '{commandLine.Target}' is not connected.");
            }
            else if (matchingClients.Length > 1) {
                _console.WriteLineError($"Multiple client are matching '{commandLine.Target}'.");
            }
            else {
                var targetId = ClientId.Parse(matchingClients.Single().Id);
                var pingEllapsedTime = _remoteClient.Ping(targetId);
                _console.WriteLine($"Ok. {pingEllapsedTime} ms.");
            }
        }
    }
}