using System.IO;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class DownloadFileCommandLineHandler : ICommandLineHandler<DownloadFileCommandLine>
    {
        private readonly IConsole _console;
        private readonly Server _server;

        public DownloadFileCommandLineHandler(IConsole console, Server server)
        {
            _console = console;
            _server = server;
        }

        public async Task Handle(DownloadFileCommandLine commandLine)
        {
            var client = await _server.FindRemoteClient(commandLine.Target);
            var downloader = client.GetFileDownloader();
            _console.WriteLine("Download started.");
            var task = downloader.DownloadAsync(commandLine.RemoteSourceFilePath, commandLine.LocalDestinationFilePath);
            while (task.Wait(500) == false) {
                _console.SetCursorLeft(0);
                _console.Write($"Progress: {downloader.Pourcentage}%");
            }
            _console.SetCursorLeft(0);
            _console.WriteLine("Progress: 100%");
            _console.WriteLine($"'{Path.GetFileName(commandLine.RemoteSourceFilePath)}' has been correctly downloaded from client '{client.Information.MachineName}' at location '{commandLine.LocalDestinationFilePath}'.");
        }
    }
}