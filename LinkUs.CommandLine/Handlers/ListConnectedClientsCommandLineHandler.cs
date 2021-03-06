﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.Commands;
using LinkUs.Core;
using LinkUs.Responses;

namespace LinkUs.CommandLine.Handlers
{
    public class ListConnectedClientsCommandLineHandler : ICommandLineHandler<ListConnectedClient>
    {
        private readonly IConsole _console;
        private readonly Server _server;

        // ----- Constructor
        public ListConnectedClientsCommandLineHandler(IConsole console, Server server)
        {
            _console = console;
            _server = server;
        }

        // ----- Public methods
        public async Task Handle(ListConnectedClient commandLine)
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