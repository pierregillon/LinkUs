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
        private readonly Server _server;

        public ModuleCommandHandler(IConsole console, Server server)
        {
            _console = console;
            _server = server;
        }

        public async Task Handle(LoadModuleCommandLine commandLine)
        {
            var remoteClient = await _server.FindRemoteClient(commandLine.Target);
            var isSucceded = await remoteClient.LoadModule(commandLine.ModuleName);
            if (!isSucceded) {
                _console.WriteLine($"Failed to load module {commandLine.ModuleName}.");
            }
        }
        public async Task Handle(UnloadModuleCommandLine commandLine)
        {
            var remoteClient = await _server.FindRemoteClient(commandLine.Target);
            var isSucceded = await remoteClient.UnLoadModule(commandLine.ModuleName);
            if (!isSucceded) {
                _console.WriteLine($"Failed to load module {commandLine.ModuleName}.");
            }
        }
        public async Task Handle(ListModulesCommandLine commandLine)
        {
            var remoteClient = await _server.FindRemoteClient(commandLine.Target);
            var modules = await remoteClient.GetModules();
            _console.WriteObjects(modules);
        }
    }
}