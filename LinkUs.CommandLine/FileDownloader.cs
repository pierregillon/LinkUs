using System;
using System.IO;
using System.Threading;
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
            var startedEvent = await _commandSender.ExecuteAsync<StartFileDownload, FileUploaderStarted>(command, _clientId);

            var writingTask = Task.Factory.StartNew(() => {
                var totalBytesTransferred = 0;
                using (var stream = File.OpenWrite(localDestinationFilePath)) {
                    while (totalBytesTransferred != startedEvent.TotalLength) {
                        var nextDataReceived = _commandSender.Receive<SendNextFileData>(_clientId, data => data.Id == startedEvent.Id).Result;
                        stream.Write(nextDataReceived.Buffer, 0, nextDataReceived.Buffer.Length);
                        totalBytesTransferred += nextDataReceived.Buffer.Length;
                        Pourcentage = (int) (totalBytesTransferred * 100 / startedEvent.TotalLength);
                    }
                }
            });
            Thread.Sleep(500);
            _commandSender.ExecuteAsync(new ReadToReceiveFileData() {Id = startedEvent.Id}, _clientId);
            await writingTask;
        }
    }
}