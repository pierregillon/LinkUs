using System;
using System.Linq;
using LinkUs.CommandLine.Verbs;
using LinkUs.Core;
using LinkUs.Core.Connection;

namespace LinkUs.CommandLine.Handlers
{
    public class PingCommandLineHandler : IHandler<PingCommandLine>
    {
        private readonly RemoteClient _remoteClient;
        private readonly Server _server;

        public PingCommandLineHandler(CommandDispatcher commandDispatcher)
        {
            _remoteClient = new RemoteClient(commandDispatcher);
            _server = new Server(commandDispatcher);
        }

        public void Handle(PingCommandLine commandLine)
        {
            var clients = _server.GetConnectedClients();
            var matchingClients = clients.Where(x => x.Id.StartsWith(commandLine.Target)).ToArray();
            if (matchingClients.Length == 0) {
                ConsoleExt.WriteError($"The client '{commandLine.Target}' is not connected.");
            }
            else if (matchingClients.Length > 1) {
                ConsoleExt.WriteError($"Multiple client are matching '{commandLine.Target}'.");
            }
            else {
                var targetId = ClientId.Parse(matchingClients.Single().Id);
                var pingEllapsedTime = _remoteClient.Ping(targetId);
                Console.WriteLine($"Ok. {pingEllapsedTime} ms.");
            }
        }
    }
}