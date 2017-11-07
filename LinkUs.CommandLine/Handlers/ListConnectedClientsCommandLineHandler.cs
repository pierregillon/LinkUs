using System;
using System.Collections.Generic;
using System.Linq;
using LinkUs.Core;
using LinkUs.Responses;

namespace LinkUs.CommandLine.Handlers
{
    public class ListConnectedClientsCommandLineHandler : IHandler<ListConnectedClient>
    {
        private readonly CommandDispatcher _commandDispatcher;

        // ----- Constructor
        public ListConnectedClientsCommandLineHandler(CommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        // ----- Public methods
        public void Handle(ListConnectedClient commandLine)
        {
            var clients = _commandDispatcher.ExecuteAsync<ListConnectedClient, ConnectedClient[]>(commandLine).Result.ToList();
            ReduceHashId(clients);
            ConsoleExt.WriteObjects(clients);
        }

        // ----- Utils
        private static void ReduceHashId(List<ConnectedClient> clients)
        {
            var count = GetMinimumHashSize(clients.Select(x => x.Id));
            foreach (var connectedClient in clients) {
                connectedClient.Id = connectedClient.Id.Substring(0, count);
            }
        }
        private static int GetMinimumHashSize(IEnumerable<string> hashes)
        {
            var count = 5;
            for (int i = count; i < Guid.NewGuid().ToString().Length; i++) {
                var ids = hashes.Select(x => x.Substring(i)).ToArray();
                if (ids.Distinct().Count() == ids.Count()) {
                    count = i;
                    break;
                }
            }
            return count;
        }
    }
}