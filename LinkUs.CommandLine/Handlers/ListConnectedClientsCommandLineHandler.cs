using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.Verbs;
using LinkUs.Responses;

namespace LinkUs.CommandLine.Handlers
{
    public class ListConnectedClientsCommandLineHandler : ICommandLineHandler<ListConnectedClientsCommandLine>
    {
        private readonly IConsole _console;
        private readonly ModuleIntegration.Default.Server _server;

        // ----- Constructor
        public ListConnectedClientsCommandLineHandler(IConsole console, ModuleIntegration.Default.Server server)
        {
            _console = console;
            _server = server;
        }

        // ----- Public methods
        public async Task Handle(ListConnectedClientsCommandLine commandLine)
        {
            var clients = await _server.GetConnectedClients();
            var clientList = clients.ToList();
            ReduceHashId(clientList);
            _console.WriteObjects(clientList);
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