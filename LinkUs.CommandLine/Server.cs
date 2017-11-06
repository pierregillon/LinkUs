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

        public string GetConnectedClients()
        {
            var defaultCommand = new ListRemoteClients();
            return _commandDispatcher.ExecuteAsync<ListRemoteClients, string>(defaultCommand).Result;
        }
    }
}