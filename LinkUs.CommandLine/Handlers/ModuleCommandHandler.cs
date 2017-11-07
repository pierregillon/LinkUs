using System;
using LinkUs.CommandLine.Verbs;
using LinkUs.Core;
using LinkUs.Core.Connection;

namespace LinkUs.CommandLine.Handlers
{
    public class ModuleCommandHandler :
        IHandler<LoadModuleCommandLine>,
        IHandler<UnloadModuleCommandLine>,
        IHandler<ListModulesCommandLine>
    {
        private readonly RemoteClient _remoteClient;

        public ModuleCommandHandler(CommandDispatcher commandDispatcher)
        {
            _remoteClient = new RemoteClient(commandDispatcher);
        }

        public void Handle(LoadModuleCommandLine commandLine)
        {
            var target = ClientId.Parse(commandLine.Target);
            var isSucceded = _remoteClient.LoadModule(target, commandLine.ModuleName);
            if (!isSucceded) {
                Console.WriteLine($"Failed to load module {commandLine.ModuleName}.");
            }
        }
        public void Handle(UnloadModuleCommandLine commandLine)
        {
            var targetId = ClientId.Parse(commandLine.Target);
            var isSucceded = _remoteClient.UnLoadModule(targetId, commandLine.ModuleName);
            if (!isSucceded) {
                Console.WriteLine($"Failed to load module {commandLine.ModuleName}.");
            }
        }
        public void Handle(ListModulesCommandLine commandLine)
        {
            var targetId = ClientId.Parse(commandLine.Target);
            var response = _remoteClient.GetModules(targetId);
            Console.WriteLine(string.Join(Environment.NewLine, response));
        }
    }
}