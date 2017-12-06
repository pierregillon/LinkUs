using System;
using System.Threading.Tasks;
using LinkUs.Client.ClientInformation;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.ModuleIntegration.Default;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class StatusCommandLineHandler : ICommandLineHandler<StatusCommandLine>
    {
        private readonly IConsole _console;
        private readonly RemoteServer _server;

        public StatusCommandLineHandler(IConsole console, RemoteServer server)
        {
            _console = console;
            _server = server;
        }

        public async Task Handle(StatusCommandLine commandLine)
        {
            var remoteClient = await _server.FindRemoteClient(commandLine.Target);
            var result = await remoteClient.ExecuteAsync<GetStatus, ClientStatus>(new GetStatus());

            _console.WriteLine($"Path\t\t{result.ClientExeLocation}");

            _console.Write("Is installed?\t");
            if (result.IsInstalled) {
                _console.WriteLineWithColor("[OK]", ConsoleColor.Green);
            }
            else {
                _console.WriteLineWithColor("[KO]", ConsoleColor.Red);
            }

            _console.Write("Is at startup?\t");
            if (result.IsRegisteredAtStartup) {
                _console.WriteLineWithColor("[OK]", ConsoleColor.Green);
            }
            else {
                _console.WriteLineWithColor("[KO]", ConsoleColor.Red);
            }
        }
    }
}