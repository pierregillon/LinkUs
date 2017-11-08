using System.Threading.Tasks;
using LinkUs.Core;
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
    }
}