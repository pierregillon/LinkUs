using System;
using System.Threading.Tasks;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class ModuleCommandHandler : 
        ICommandLineHandler<LoadModuleCommandLine>,
        ICommandLineHandler<UnloadModuleCommandLine>,
        ICommandLineHandler<ListModulesCommandLine>
    {
        private readonly RemoteClient _remoteClient;
        private readonly Server _server;

        public ModuleCommandHandler(RemoteClient remoteClient, Server server)
        {
            _remoteClient = remoteClient;
            _server = server;
        }

        public async Task Handle(LoadModuleCommandLine commandLine)
        {
            var targetId = await _server.FindCliendId(commandLine.Target);
            var isSucceded = await _remoteClient.LoadModule(targetId, commandLine.ModuleName);
            if (!isSucceded) {
                Console.WriteLine($"Failed to load module {commandLine.ModuleName}.");
            }
        }
        public async Task Handle(UnloadModuleCommandLine commandLine)
        {
            var targetId = await _server.FindCliendId(commandLine.Target);
            var isSucceded = await _remoteClient.UnLoadModule(targetId, commandLine.ModuleName);
            if (!isSucceded) {
                Console.WriteLine($"Failed to load module {commandLine.ModuleName}.");
            }
        }
        public async Task Handle(ListModulesCommandLine commandLine)
        {
            var targetId = await _server.FindCliendId(commandLine.Target);
            var response = await _remoteClient.GetModules(targetId);
            Console.WriteLine(string.Join(Environment.NewLine, response));
        }
    }
}