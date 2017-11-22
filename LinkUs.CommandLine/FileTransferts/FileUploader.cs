using System;
using System.IO;
using System.Threading.Tasks;
using LinkUs.CommandLine.ModuleIntegration.Default;
using LinkUs.Core.Commands;
using LinkUs.Modules.Default.FileTransfert.Commands;
using LinkUs.Modules.Default.FileTransfert.Events;

namespace LinkUs.CommandLine.FileTransferts
{
    public class FileUploader : IProgressable
    {
        private readonly IDedicatedCommandSender _client;

        public int Pourcentage { get; private set; }

        // ----- Constructor
        public FileUploader(IDedicatedCommandSender client)
        {
            _client = client;
        }

        // ----- Public methods
        public async Task UploadAsync(string sourceFilePath, string destinationFilePath)
        {
            if (File.Exists(sourceFilePath) == false) {
                throw new Exception($"Unable to start the upload: the file path '{sourceFilePath}' is invalid.");
            }
            var fileLength = new FileInfo(sourceFilePath).Length;
            var startedEvent = await StartUpload(destinationFilePath, fileLength);
            await UploadFile(sourceFilePath, startedEvent.FileId, fileLength);
            await EndUpload(startedEvent.FileId);
        }

        // ----- Internal logic
        private Task<FileUploadStarted> StartUpload(string destinationFilePath, long fileLength)
        {
            var startCommand = new StartFileUpload {
                DestinationFilePath = destinationFilePath,
                Length = fileLength
            };
            return _client.ExecuteAsync<StartFileUpload, FileUploadStarted>(startCommand);
        }
        private async Task UploadFile(string sourceFilePath, Guid fileId, long fileLength)
        {
            try {
                var totalBytesTransferred = 0;
                await ForEachFileRead(sourceFilePath, async buffer => {
                    await SendNextFileDataCommand(fileId, buffer);
                    totalBytesTransferred += buffer.Length;
                    Pourcentage = (int) (totalBytesTransferred * 100 / fileLength);
                });
            }
            catch (ExecuteCommandTimeoutException) {
                throw;
            }
            catch (Exception) {
                await EndUpload(fileId);
                throw;
            }
        }
        private async Task SendNextFileDataCommand(Guid fileId, byte[] buffer)
        {
            var sendCommand = new SendNextFileData {
                FileId = fileId,
                Buffer = buffer
            };
            await _client.ExecuteAsync<SendNextFileData, bool>(sendCommand);
        }
        private async Task EndUpload(Guid fileId)
        {
            var endCommand = new EndFileUpload { FileId = fileId };
            await _client.ExecuteAsync<EndFileUpload, FileUploadEnded>(endCommand);
        }

        // ----- Utils
        private static async Task ForEachFileRead(string sourceFilePath, Func<byte[], Task> action)
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
    }
}