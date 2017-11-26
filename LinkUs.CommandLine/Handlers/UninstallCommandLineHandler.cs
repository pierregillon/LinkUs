using System.Threading.Tasks;
using LinkUs.Client.Install;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.ModuleIntegration.Default;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class UninstallCommandLineHandler : ICommandLineHandler<UninstallCommandLine>
    {
        private readonly IConsole _console;
        private readonly RemoteServer _server;

        public UninstallCommandLineHandler(IConsole console, RemoteServer server)
        {
            _console = console;
            _server = server;
        }

        public async Task Handle(UninstallCommandLine commandLine)
        {
            _console.WriteLine("Are you sure to uninstall the client? " +
                               "This operation is irremediable and makes " +
                               "no future connection possible to the client.");
            _console.Write("Yes (y) or No (n) => ");
            var answer = _console.ReadLine();
            if (answer == "y") {
                var remoteClient = await _server.FindRemoteClient(commandLine.Target);
                await remoteClient.ExecuteAsync<UninstallClient, bool>(new UninstallClient());
            }
        }
    }
}