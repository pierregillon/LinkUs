using System;
using System.IO;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class UploadFileCommandLineHandler : ICommandLineHandler<UploadFileCommandLine>
    {
        private readonly IConsole _console;
        private readonly ModuleIntegration.Default.Server _server;

        public UploadFileCommandLineHandler(IConsole console, ModuleIntegration.Default.Server server)
        {
            _console = console;
            _server = server;
        }

        public async Task Handle(UploadFileCommandLine commandLine)
        {
            var client = await _server.FindRemoteClient(commandLine.Target);
            var uploader = client.GetFileUploader();
            _console.WriteLine("Upload started.");
            var task = uploader.UploadAsync(commandLine.LocalSourceFilePath, commandLine.RemoteDestinationFilePath);
            _console.WriteProgress(task, uploader);
            _console.WriteLine($"'{Path.GetFullPath(commandLine.LocalSourceFilePath)}' has been correctly uploaded to client '{client.Information.MachineName}' at location '{commandLine.RemoteDestinationFilePath}'.");
        }
    }
}