using System;
using System.IO;
using System.Threading.Tasks;
using LinkUs.Core;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Core.Packages;
using LinkUs.Modules.Default.FileTransfert.Commands;
using LinkUs.Modules.Default.FileTransfert.Events;

namespace LinkUs.CommandLine.FileTransferts
{
    public class FileUploader : IProgressable
    {
        private readonly ICommandSender _commandSender;
        private readonly ClientId _clientId;

        public int Pourcentage { get; private set; }

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
            var startCommand = new StartFileUpload {
                DestinationFilePath = destinationFilePath,
                Length = new FileInfo(sourceFilePath).Length
            };
            var startedEvent = await _commandSender.ExecuteAsync<StartFileUpload, FileUploadStarted>(startCommand, _clientId);
            await SendSourceFile(sourceFilePath, startedEvent.FileId, startCommand.Length);
            var endCommand = new EndFileUpload { FileId = startedEvent.FileId };
            await _commandSender.ExecuteAsync<EndFileUpload, FileUploadEnded>(endCommand, _clientId);
        }

        private async Task SendSourceFile(string sourceFilePath, Guid fileId, long fileLength)
        {
            var totalBytesTransferred = 0;
            await ForEachFileRead(sourceFilePath, async buffer => {
                await SendNextFileDataCommand(fileId, buffer);
                totalBytesTransferred += buffer.Length;
                Pourcentage = (int) (totalBytesTransferred * 100 / fileLength);
            });
        }

        private async Task ForEachFileRead(string sourceFilePath, Func<byte[], Task> action)
        {
            var buffer = new byte[1024];
            using (var stream = File.OpenRead(sourceFilePath)) {
                int bytesReadCount;
                do {
                    bytesReadCount = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesReadCount != 0) {
                        await action(GetExactDataBuffer(bytesReadCount, buffer));
                    }
                } while (bytesReadCount != 0);
            }
        }
        private static byte[] GetExactDataBuffer(int bytesReadCount, byte[] buffer)
        {
            if (bytesReadCount != buffer.Length) {
                var endBuffer = new byte[bytesReadCount];
                Buffer.BlockCopy(buffer, 0, endBuffer, 0, bytesReadCount);
                buffer = endBuffer;
            }
            return buffer;
        }
        private async Task SendNextFileDataCommand(Guid fileId, byte[] buffer)
        {
            var sendCommand = new SendNextFileData {
                FileId = fileId,
                Buffer = buffer
            };
            await _commandSender.ExecuteAsync<SendNextFileData, bool>(sendCommand, _clientId);
        }
    }
}