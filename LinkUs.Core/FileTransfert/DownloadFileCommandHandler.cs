using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using LinkUs.Core.FileTransfert.Commands;
using LinkUs.Core.FileTransfert.Events;

namespace LinkUs.Core.FileTransfert
{
    public class DownloadFileCommandHandler :
        IHandler<StartFileDownload, FileDownloadStarted>,
        IHandler<ReadyToReceiveFileData, FileDownloadEnded>
    {
        private static readonly IDictionary<Guid, Stream> OpenedStreams = new ConcurrentDictionary<Guid, Stream>();
        private readonly IBus _bus;

        public DownloadFileCommandHandler(IBus bus)
        {
            _bus = bus;
        }

        public FileDownloadStarted Handle(StartFileDownload command)
        {
            try {
                var id = Guid.NewGuid();
                var stream = File.Open(command.SourceFilePath, FileMode.Open);
                OpenedStreams.Add(id, stream);
                return new FileDownloadStarted {
                    FileId = id,
                    TotalLength = stream.Length
                };
            }
            catch (Exception ex) {
                throw new Exception($"Unable to start the download of the file '{command.SourceFilePath}'.", ex);
            }
        }

        public FileDownloadEnded Handle(ReadyToReceiveFileData command)
        {
            Stream stream;
            if (OpenedStreams.TryGetValue(command.FileId, out stream) == false) {
                throw new Exception($"Unable to find the file with id '{command.FileId}'.");
            }

            try {
                var buffer = new byte[1024];
                while (true) {
                    var bytesReadCount = stream.Read(buffer, 0, buffer.Length);
                    if (bytesReadCount == 0) {
                        break;
                    }
                    if (bytesReadCount < buffer.Length) {
                        var endBuffer = new byte[bytesReadCount];
                        Buffer.BlockCopy(buffer, 0, endBuffer, 0, bytesReadCount);
                        buffer = endBuffer;
                    }
                    _bus.Send(new SendNextFileData {
                        FileId = command.FileId,
                        Buffer = buffer
                    });
                }
            }
            catch (Exception ex) {
                throw new Exception("An unexpected error occurred during the file download.", ex);
            }
            finally {
                stream.Close();
                OpenedStreams.Remove(command.FileId);
            }

            return new FileDownloadEnded() { FileId = command.FileId };
        }
    }
}