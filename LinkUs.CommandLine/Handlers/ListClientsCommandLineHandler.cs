using System;
using LinkUs.Core;

namespace LinkUs.CommandLine.Handlers
{
    public class ListClientsCommandLineHandler:IHandler<ListRemoteClients>
    {
        private readonly CommandDispatcher _commandDispatcher;

        public ListClientsCommandLineHandler(CommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        public void Handle(ListRemoteClients command)
        {
            var result = _commandDispatcher.ExecuteAsync<ListRemoteClients, string>(command).Result;
            Console.WriteLine(result);
        }
    }
}
