using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LinkUs.Core.FileTransfert.Commands;
using LinkUs.Core.FileTransfert.Events;

namespace LinkUs.Core.FileTransfert
{
    public class DownloadFileCommandHandler :
        IHandler<StartFileDownload, FileDownloadStarted>,
        IHandler<GetNextFileData, NextFileDataRead>,
        IHandler<EndFileDownload, FileDownloadEnded>
    {
        private static readonly IDictionary<Guid, Stream> OpenedStreams = new ConcurrentDictionary<Guid, Stream>();

        // ----- Public methods
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
        public NextFileDataRead Handle(GetNextFileData command)
        {
            var stream = GetOpenedStream(command.FileId);
            try {
                var buffer = new byte[1024];
                var bytesReadCount = stream.Read(buffer, 0, buffer.Length);
                if (bytesReadCount != buffer.Length) {
                    buffer = buffer.Take(bytesReadCount).ToArray();
                }
                return new NextFileDataRead {
                    FileId = command.FileId,
                    Data = buffer
                };
            }
            catch (Exception ex) {
                stream.Close();
                OpenedStreams.Remove(command.FileId);
                throw new Exception("An unexpected error occurred during the file download.", ex);
            }
        }
        public FileDownloadEnded Handle(EndFileDownload command)
        {
            var fileStream = GetOpenedStream(command.FileId);
            fileStream.Close();
            fileStream.Dispose();
            OpenedStreams.Remove(command.FileId);
            return new FileDownloadEnded {
                FileId = command.FileId
            };
        }

        // ----- Utils
        private static Stream GetOpenedStream(Guid fileId)
        {
            Stream stream;
            if (OpenedStreams.TryGetValue(fileId, out stream) == false) {
                throw new Exception($"Unable to download, the file id '{fileId}' does not exist.");
            }
            return stream;
        }
    }
}