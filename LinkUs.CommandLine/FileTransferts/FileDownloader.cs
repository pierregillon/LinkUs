using System;
using System.IO;
using System.Threading.Tasks;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.FileTransfert.Commands;
using LinkUs.Core.FileTransfert.Events;

namespace LinkUs.CommandLine.FileTransferts
{
    public class FileDownloader : IProgressable
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

            var startedEvent = await StartDownload(remoteSourceFilePath);
            var totalBytesTransferred = await CopyDataToLocalFile(startedEvent, localDestinationFilePath);
            await EndDownload(startedEvent);

            if (totalBytesTransferred != startedEvent.TotalLength) {
                throw new Exception("The total amount of bytes received is not correct, file must be corrupted.");
            }
        }

        // ----- Internal logics
        private async Task<FileDownloadStarted> StartDownload(string remoteSourceFilePath)
        {
            var startCommand = new StartFileDownload { SourceFilePath = remoteSourceFilePath };
            var startedEvent = await _commandSender.ExecuteAsync<StartFileDownload, FileDownloadStarted>(startCommand, _clientId);
            return startedEvent;
        }
        private async Task<int> CopyDataToLocalFile(FileDownloadStarted startedEvent, string filePath)
        {
            var totalBytesTransferred = 0;
            using (var stream = File.OpenWrite(filePath)) {
                var nextFileDataCommand = new GetNextFileData { FileId = startedEvent.FileId };
                while (true) {
                    var fileDataRead = await _commandSender.ExecuteAsync<GetNextFileData, NextFileDataRead>(nextFileDataCommand, _clientId);
                    if (fileDataRead.Data.Length == 0) {
                        break;
                    }
                    stream.Write(fileDataRead.Data, 0, fileDataRead.Data.Length);
                    totalBytesTransferred += fileDataRead.Data.Length;
                    Pourcentage = (int) (totalBytesTransferred * 100 / startedEvent.TotalLength);
                }
            }
            return totalBytesTransferred;
        }
        private async Task EndDownload(FileDownloadStarted startedEvent)
        {
            var endCommand = new EndFileDownload { FileId = startedEvent.FileId };
            await _commandSender.ExecuteAsync<EndFileDownload, FileDownloadEnded>(endCommand, _clientId);
        }
    }
}