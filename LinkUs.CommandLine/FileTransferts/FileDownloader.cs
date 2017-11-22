using System;
using System.IO;
using System.Threading.Tasks;
using LinkUs.CommandLine.ModuleIntegration.Default;
using LinkUs.Core.Commands;
using LinkUs.Modules.Default.FileTransfert.Commands;
using LinkUs.Modules.Default.FileTransfert.Events;

namespace LinkUs.CommandLine.FileTransferts
{
    public class FileDownloader : IProgressable
    {
        private readonly IDedicatedCommandSender _client;
        public int Pourcentage { get; private set; }

        // ----- Constructors
        public FileDownloader(IDedicatedCommandSender client)
        {
            _client = client;
        }

        // ----- Public methods
        public async Task DownloadAsync(string remoteSourceFilePath, string localDestinationFilePath)
        {
            PrepareFileLocation(localDestinationFilePath);

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
            var startedEvent = await _client.ExecuteAsync<StartFileDownload, FileDownloadStarted>(startCommand);
            return startedEvent;
        }
        private async Task<int> CopyDataToLocalFile(FileDownloadStarted startedEvent, string filePath)
        {
            try {
                var totalBytesTransferred = 0;
                using (var stream = File.OpenWrite(filePath)) {
                    var nextFileDataCommand = new GetNextFileData { FileId = startedEvent.FileId };
                    while (true) {
                        var fileDataRead = await _client.ExecuteAsync<GetNextFileData, NextFileDataRead>(nextFileDataCommand);
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
            catch (ExecuteCommandTimeoutException) {
                throw;
            }
            catch (Exception) {
                await EndDownload(startedEvent);
                throw;
            }
        }
        private async Task EndDownload(FileDownloadStarted startedEvent)
        {
            var endCommand = new EndFileDownload { FileId = startedEvent.FileId };
            await _client.ExecuteAsync<EndFileDownload, FileDownloadEnded>(endCommand);
        }

        // ----- Utils
        private static void PrepareFileLocation(string localDestinationFilePath)
        {
            if (File.Exists(localDestinationFilePath)) {
                File.Delete(localDestinationFilePath);
            }
            var parent = Directory.GetParent(localDestinationFilePath).FullName;
            if (string.IsNullOrEmpty(parent) == false) {
                Directory.CreateDirectory(parent);
            }
        }
    }
}