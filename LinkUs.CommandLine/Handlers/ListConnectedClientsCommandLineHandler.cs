using System;
using LinkUs.Core;

namespace LinkUs.CommandLine.Handlers
{
    public class ListConnectedClientsCommandLineHandler:IHandler<ListConnectedClient>
    {
        private readonly CommandDispatcher _commandDispatcher;

        public ListConnectedClientsCommandLineHandler(CommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        public void Handle(ListConnectedClient commandLine)
        {
            var clients = _commandDispatcher.ExecuteAsync<ListConnectedClient, ConnectedClient[]>(commandLine).Result;
            foreach (var connectedClient in clients) {
                Console.WriteLine($"{connectedClient.Id}\t{connectedClient.MachineName}\t{connectedClient.Ip}");
            }
        }
    }
}
