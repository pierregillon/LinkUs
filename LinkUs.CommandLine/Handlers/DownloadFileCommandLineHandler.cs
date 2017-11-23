using System.IO;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.ModuleIntegration.Default.FileTransferts;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class DownloadFileCommandLineHandler : ICommandLineHandler<DownloadFileCommandLine>
    {
        private readonly IConsole _console;
        private readonly ModuleIntegration.Default.RemoteServer _server;

        public DownloadFileCommandLineHandler(IConsole console, ModuleIntegration.Default.RemoteServer server)
        {
            _console = console;
            _server = server;
        }

        public async Task Handle(DownloadFileCommandLine commandLine)
        {
            var client = await _server.FindRemoteClient(commandLine.Target);
            var downloader = new FileDownloader(client);
            var resultFilePath = await _console.WriteProgress("Downloading file", downloader, downloader.DownloadAsync(commandLine.RemoteSourceFilePath, commandLine.LocalDestinationFilePath));
            _console.WriteLine($"'{Path.GetFileName(commandLine.RemoteSourceFilePath)}' has been correctly downloaded from client '{client.Information.MachineName}' at location '{Path.GetFullPath(resultFilePath)}'.");
        }
    }
}