using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.Verbs;
using LinkUs.Core.Connection;

namespace LinkUs.CommandLine.Handlers
{
    public class UploadCommandLineHandler : ICommandLineHandler<UploadCommandLine>
    {
        private readonly IConsole _console;
        private readonly Server _server;

        public UploadCommandLineHandler(IConsole console, Server server)
        {
            _console = console;
            _server = server;
        }

        public async Task Handle(UploadCommandLine commandLine)
        {
            var client = await _server.FindRemoteClient(commandLine.Target);
            var uploader = client.GetFileUploader();
            _console.WriteLine("Upload started.");
            var task = uploader.UploadAsync(commandLine.SourceFilePath, commandLine.DestinationFilePath);
            while (task.Wait(500) == false) {
                _console.MoveCursorLeft(0);
                _console.Write($"Progress: {uploader.Pourcentage}%");
            }
            _console.WriteLine("Progress: 100%");
            _console.WriteLine($"'{Path.GetFileName(commandLine.SourceFilePath)}' has been correctly uploaded to client '{client.Information.MachineName}' at location '{commandLine.DestinationFilePath}'.");
        }
    }
}