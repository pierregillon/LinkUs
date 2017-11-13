using System;
using System.IO;
using System.Threading.Tasks;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.FileTransfert.Commands;
using LinkUs.Core.FileTransfert.Events;

namespace LinkUs.CommandLine
{
    public class FileDownloader
    {
        private readonly ICommandSender _commandSender;
        private readonly ClientId _clientId;
        public int Pourcentage { get; private set; }

        public FileDownloader(ICommandSender commandSender, ClientId clientId)
        {
            _commandSender = commandSender;
            _clientId = clientId;
        }

        public async Task DownloadAsync(string remoteSourceFilePath, string localDestinationFilePath)
        {
            if (File.Exists(localDestinationFilePath)) {
                File.Delete(localDestinationFilePath);
            }
            var command = new StartFileDownload {
                SourceFilePath = remoteSourceFilePath
            };

            var startedEvent = await _commandSender.ExecuteAsync<StartFileDownload, FileDownloadStarted>(command, _clientId);
            var dataStream = _commandSender.BuildStream<SendNextFileData>(data => data.FileId == startedEvent.FileId);
            dataStream.Start();

            _commandSender.ExecuteAsync(new ReadyToReceiveFileData {FileId = startedEvent.FileId}, _clientId);

            var endedEventTask =_commandSender
                .Receive<FileDownloadEnded>(_clientId, @event => @event.FileId == startedEvent.FileId)
                .ContinueWith(x => dataStream.End());

            var totalBytesTransferred = 0;
            using (var stream = File.OpenWrite(localDestinationFilePath)) {
                foreach (var sendNextFileData in dataStream.GetData()) {
                    stream.Write(sendNextFileData.Buffer, 0, sendNextFileData.Buffer.Length);
                    totalBytesTransferred += sendNextFileData.Buffer.Length;
                    Pourcentage = (int) (totalBytesTransferred * 100 / startedEvent.TotalLength);
                }
            }

            await endedEventTask;

            if (totalBytesTransferred != startedEvent.TotalLength) {
                throw new Exception("The total amount of bytes received is not correct, file must be corrupted.");
            }
        }
    }
}