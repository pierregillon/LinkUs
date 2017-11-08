using System;
using System.Linq;
using System.Threading.Tasks;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Responses;

namespace LinkUs.CommandLine
{
    public class Server
    {
        private readonly ICommandSender _commandSender;

        public Server(ICommandSender commandSender)
        {
            _commandSender = commandSender;
        }

        public Task<ConnectedClient[]> GetConnectedClients()
        {
            var command = new ListConnectedClient();
            return _commandSender.ExecuteAsync<ListConnectedClient, ConnectedClient[]>(command);
        }
        public async Task<ClientId> FindCliendId(string partialClientId)
        {
            var client = await GetConnectedClient(partialClientId);
            return ClientId.Parse(client.Id);
        }
        public async Task<ConnectedClient> GetConnectedClient(string partialClientId)
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