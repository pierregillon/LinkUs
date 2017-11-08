using System;
using System.Threading.Tasks;
using LinkUs.CommandLine.Verbs;
using LinkUs.Core;
using LinkUs.Core.Connection;

namespace LinkUs.CommandLine.Handlers
{
    public class ModuleCommandHandler : PartialClientIdHandler,
        ICommandLineHandler<LoadModuleCommandLine>,
        ICommandLineHandler<UnloadModuleCommandLine>,
        ICommandLineHandler<ListModulesCommandLine>
    {
        private readonly RemoteClient _remoteClient;

        public ModuleCommandHandler(RemoteClient remoteClient, Server server) : base(server)
        {
            _remoteClient = remoteClient;
        }

        public async Task Handle(LoadModuleCommandLine commandLine)
        {
            var targetId = await FindCliendId(commandLine.Target);
            var isSucceded = await _remoteClient.LoadModule(targetId, commandLine.ModuleName);
            if (!isSucceded) {
                Console.WriteLine($"Failed to load module {commandLine.ModuleName}.");
            }
        }
        public async Task Handle(UnloadModuleCommandLine commandLine)
        {
            var targetId = await FindCliendId(commandLine.Target);
            var isSucceded = await _remoteClient.UnLoadModule(targetId, commandLine.ModuleName);
            if (!isSucceded) {
                Console.WriteLine($"Failed to load module {commandLine.ModuleName}.");
            }
        }
        public async Task Handle(ListModulesCommandLine commandLine)
        {
            var targetId = await FindCliendId(commandLine.Target);
            var response = await _remoteClient.GetModules(targetId);
            Console.WriteLine(string.Join(Environment.NewLine, response));
        }
    }
}