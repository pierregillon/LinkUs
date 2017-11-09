using System;
using System.IO;
using System.Threading.Tasks;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.FileTransfert.Commands;
using LinkUs.Core.FileTransfert.Events;

namespace LinkUs.CommandLine
{
    public class FileUploader
    {
        private readonly ICommandSender _commandSender;
        private readonly ClientId _clientId;

        public FileUploader(
            ICommandSender commandSender,
            ClientId clientId)
        {
            _commandSender = commandSender;
            _clientId = clientId;
        }

        public async Task UploadAsync(string sourceFilePath, string destinationFilePath)
        {
            if (File.Exists(sourceFilePath) == false) {
                throw new Exception($"Unable to start the upload: the file path '{sourceFilePath}' is invalid.");
            }
            var command = new StartFileUpload {
                DestinationFilePath = destinationFilePath,
                Length = new FileInfo(sourceFilePath).Length
            };
            var startedEvent = await _commandSender.ExecuteAsync<StartFileUpload, FileDownloaderStarted>(command, _clientId);
            var buffer = new byte[1024];
            using (var stream = File.OpenRead(sourceFilePath)) {
                int bytesReadCount;
                do {
                    bytesReadCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesReadCount != 0) {
                        if (bytesReadCount == buffer.Length) {
                            var command2 = new SendNextFileData {
                                Id = startedEvent.Id,
                                Buffer = buffer
                            };
                            _commandSender.ExecuteAsync(command2, _clientId);
                        }
                        else {
                            var endBuffer = new byte[bytesReadCount];
                            Buffer.BlockCopy(buffer, 0, endBuffer, 0, bytesReadCount);
                            var command2 = new SendNextFileData
                            {
                                Id = startedEvent.Id,
                                Buffer = endBuffer
                            };
                            _commandSender.ExecuteAsync(command2, _clientId);
                        }
                        
                    }
                } while (bytesReadCount != 0);
            }

            await _commandSender.Receive<FileDownloaderEnded>(_clientId, @event => @event.Id == startedEvent.Id);
        }
    }
}