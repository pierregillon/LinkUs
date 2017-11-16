using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkUs.Commands;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.FileTransfert.Commands;
using LinkUs.Responses;

namespace LinkUs.CommandLine
{
    public class Server
    {
        private readonly ICommandSender _commandSender;

        // ----- Constructor
        public Server(ICommandSender commandSender)
        {
            _commandSender = commandSender;
        }

        // ----- Public methods
        public async Task<IReadOnlyCollection<ConnectedClient>> GetConnectedClients()
        {
            var command = new ListConnectedClient();
            var connectedClients = await _commandSender.ExecuteAsync<ListConnectedClient, ConnectedClient[]>(command);
            return connectedClients;
        }
        public async Task<RemoteClient> FindRemoteClient(string partialClientId)
        {
            var clientInformation = await GetConnectedClient(partialClientId);
            return new RemoteClient(_commandSender, clientInformation);
        }

        // ----- Utils
        private async Task<ConnectedClient> GetConnectedClient(string partialClientId)
        {
            var clients = await GetConnectedClients();
            var matchingClients = clients.Where(x => x.Id.StartsWith(partialClientId)).ToArray();
            if (matchingClients.Length == 0) {
                throw new Exception($"The client '{partialClientId}' is not connected.");
            }
            if (matchingClients.Length > 1) {
                throw new Exception($"Multiple client are matching '{partialClientId}'.");
            }
            return matchingClients.Single();
        }
    }
}