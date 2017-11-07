using System;
using LinkUs.CommandLine.Verbs;
using LinkUs.Core;
using LinkUs.Core.Connection;

namespace LinkUs.CommandLine.Handlers
{
    public class PingCommandLineHandler : IHandler<PingCommandLine>
    {
        private readonly RemoteClient _remoteClient;

        public PingCommandLineHandler(CommandDispatcher commandDispatcher)
        {
            _remoteClient = new RemoteClient(commandDispatcher);
        }

        public void Handle(PingCommandLine commandLine)
        {
            var targetId = ClientId.Parse(commandLine.Target);
            var pingEllapsedTime = _remoteClient.Ping(targetId);
            Console.WriteLine($"Ok. {pingEllapsedTime} ms.");
        }
    }
}
