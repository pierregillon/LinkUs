using LinkUs.Core;

namespace LinkUs.CommandLine
{
    public class Server
    {
        private readonly CommandDispatcher _commandDispatcher;

        public Server(CommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        public ConnectedClient[] GetConnectedClients()
        {
            var command = new ListConnectedClient();
            return _commandDispatcher.ExecuteAsync<ListConnectedClient, ConnectedClient[]>(command).Result;
        }
    }
}