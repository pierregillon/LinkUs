﻿using System.IO;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.FileTransferts;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class DownloadFileCommandLineHandler : ICommandLineHandler<DownloadFileCommandLine>
    {
        private readonly IConsole _console;
        private readonly ModuleIntegration.Default.Server _server;

        public DownloadFileCommandLineHandler(IConsole console, ModuleIntegration.Default.Server server)
        {
            _console = console;
            _server = server;
        }

        public async Task Handle(DownloadFileCommandLine commandLine)
        {
            var client = await _server.FindRemoteClient(commandLine.Target);
            var downloader = new FileDownloader(client);
            _console.WriteLine("Download started.");
            var task = downloader.DownloadAsync(commandLine.RemoteSourceFilePath, commandLine.LocalDestinationFilePath);
            _console.WriteProgress(task, downloader);
            _console.WriteLine($"'{Path.GetFileName(commandLine.RemoteSourceFilePath)}' has been correctly downloaded from client '{client.Information.MachineName}' at location '{Path.GetFullPath(commandLine.LocalDestinationFilePath)}'.");
        }
    }
}