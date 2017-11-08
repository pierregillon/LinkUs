using System;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class ModuleCommandHandler : 
        ICommandLineHandler<LoadModuleCommandLine>,
        ICommandLineHandler<UnloadModuleCommandLine>,
        ICommandLineHandler<ListModulesCommandLine>
    {
        private readonly IConsole _console;
        private readonly RemoteClient _remoteClient;
        private readonly Server _server;

        public ModuleCommandHandler(IConsole console, RemoteClient remoteClient, Server server)
        {
            _console = console;
            _remoteClient = remoteClient;
            _server = server;
        }

        public async Task Handle(LoadModuleCommandLine commandLine)
        {
            var targetId = await _server.FindCliendId(commandLine.Target);
            var isSucceded = await _remoteClient.LoadModule(targetId, commandLine.ModuleName);
            if (!isSucceded) {
                _console.WriteLine($"Failed to load module {commandLine.ModuleName}.");
            }
        }
        public async Task Handle(UnloadModuleCommandLine commandLine)
        {
            var targetId = await _server.FindCliendId(commandLine.Target);
            var isSucceded = await _remoteClient.UnLoadModule(targetId, commandLine.ModuleName);
            if (!isSucceded) {
                _console.WriteLine($"Failed to load module {commandLine.ModuleName}.");
            }
        }
        public async Task Handle(ListModulesCommandLine commandLine)
        {
            var targetId = await _server.FindCliendId(commandLine.Target);
            var modules = await _remoteClient.GetModules(targetId);
            _console.WriteObjects(modules);
        }
    }
}