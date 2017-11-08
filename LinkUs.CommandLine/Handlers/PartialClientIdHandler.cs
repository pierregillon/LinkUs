using System;
using System.Linq;
using System.Threading.Tasks;
using LinkUs.Core.Connection;

namespace LinkUs.CommandLine.Handlers
{
    public abstract class PartialClientIdHandler
    {
        private readonly Server _server;

        protected PartialClientIdHandler(Server server)
        {
            _server = server;
        }

        public async Task<ClientId> FindCliendId(string partialClientId)
        {
            var clients = await _server.GetConnectedClients();
            var matchingClients = clients.Where(x => x.Id.StartsWith(partialClientId)).ToArray();
            if (matchingClients.Length == 0) {
                throw new Exception($"The client '{partialClientId}' is not connected.");
            }
            if (matchingClients.Length > 1) {
                throw new Exception($"Multiple client are matching '{partialClientId}'.");
            }
            return ClientId.Parse(matchingClients.Single().Id);
        }
    }
}