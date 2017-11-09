using System.IO;
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
        private readonly RemoteClient _remoteClient;

        public UploadCommandLineHandler(IConsole console, Server server, RemoteClient remoteClient)
        {
            _console = console;
            _server = server;
            _remoteClient = remoteClient;
        }

        public async Task Handle(UploadCommandLine commandLine)
        {
            var client = await _server.GetConnectedClient(commandLine.Target);
            var uploader = _remoteClient.GetFileUploader(ClientId.Parse(client.Id));
            //uploader.Progress += avancement => _console.Write(avancement);
            _console.WriteLine("Upload started.");
            await uploader.UploadAsync(commandLine.SourceFilePath, commandLine.DestinationFilePath);
            _console.WriteLine($"{Path.GetFileName(commandLine.SourceFilePath)} has been uploaded to server.");
        }
    }
}