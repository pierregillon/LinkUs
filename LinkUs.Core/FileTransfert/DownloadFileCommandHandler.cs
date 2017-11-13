using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using LinkUs.Core.FileTransfert.Commands;
using LinkUs.Core.FileTransfert.Events;
using LinkUs.Modules.RemoteShell;

namespace LinkUs.Core.FileTransfert
{
    public class DownloadFileCommandHandler :
        IHandler<StartFileDownload>,
        IHandler<ReadyToReceiveFileData>
    {
        private static readonly IDictionary<Guid, Stream> OpenedStreams = new ConcurrentDictionary<Guid, Stream>();
        private readonly IBus _bus;

        public DownloadFileCommandHandler(IBus bus)
        {
            _bus = bus;
        }

        public void Handle(StartFileDownload command)
        {
            if (File.Exists(command.SourceFilePath) == false) {
                throw new Exception($"Unable to download '{command.SourceFilePath}': the path is invalid.");
            }
            var id = Guid.NewGuid();
            var stream = File.Open(command.SourceFilePath, FileMode.Open);
            OpenedStreams.Add(id, stream);
            _bus.Answer(new FileDownloadStarted {FileId = id, TotalLength = stream.Length});
        }

        public void Handle(ReadyToReceiveFileData command)
        {
            Stream stream;
            if (OpenedStreams.TryGetValue(command.FileId, out stream) == false) {
                throw new Exception($"Unable to find the stream with id '{command.FileId}'.");
            }

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
            stream.Close();
            OpenedStreams.Remove(command.FileId);
            _bus.Answer(new FileDownloadEnded() {FileId = command.FileId});
        }
    }
}