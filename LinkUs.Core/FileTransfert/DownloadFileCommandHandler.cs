using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LinkUs.Core.FileTransfert.Commands;
using LinkUs.Core.FileTransfert.Events;
using LinkUs.Modules.RemoteShell;

namespace LinkUs.Core.FileTransfert
{
    public class DownloadFileCommandHandler :
        IHandler<StartFileDownload>,
        IHandler<ReadyToReceiveFileData>
    {
        private static readonly IDictionary<Guid, Stream> Streams = new ConcurrentDictionary<Guid, Stream>();
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
            Streams.Add(id, stream);
            _bus.Answer(new FileUploaderStarted {Id = id, TotalLength = stream.Length});
        }

        public void Handle(ReadyToReceiveFileData command)
        {
            Stream stream;
            if (Streams.TryGetValue(command.Id, out stream) == false) {
                throw new Exception($"Unable to find the stream with id '{command.Id}'.");
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
                    Id = command.Id,
                    Buffer = buffer
                });
            }
            stream.Close();
            Streams.Remove(command.Id);
            _bus.Answer(new FileUploaderEnded() {Id = command.Id});
        }
    }
}