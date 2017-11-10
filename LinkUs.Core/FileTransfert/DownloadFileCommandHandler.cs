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
    public class DownloadFileCommandHandler
        : IHandler<StartFileDownload>
    {
        private static IDictionary<Guid, Stream> Uploaders = new ConcurrentDictionary<Guid, Stream>();
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
            using (var stream = File.Open(command.SourceFilePath, FileMode.Open)) {
                _bus.Answer(new FileUploaderStarted {Id = id, TotalLength = stream.Length});
                Thread.Sleep(100);
                var buffer = new byte[1024];
                while (true) {
                    var bytesReadCount = stream.Read(buffer, 0, buffer.Length);
                    if (bytesReadCount == 0) {
                        break;
                    }
                    if (bytesReadCount < buffer.Length) {
                        var endBuffer = new byte[bytesReadCount];
                        _bus.Send(new SendNextFileData {
                            Id = id,
                            Buffer = endBuffer
                        });
                    }
                    else {
                        _bus.Send(new SendNextFileData {
                            Id = id,
                            Buffer = buffer
                        });
                    }
                    Thread.Sleep(100);
                }
                stream.Close();
            }

            _bus.Send(new FileUploaderEnded {Id = id});
        }
    }
}