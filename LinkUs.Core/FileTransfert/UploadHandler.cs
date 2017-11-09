using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using LinkUs.Core.FileTransfert.Commands;
using LinkUs.Core.FileTransfert.Events;

namespace LinkUs.Core.FileTransfert
{
    public class UploadHandler :
        IHandler<StartFileUpload, FileDownloaderStarted>,
        IHandler<SendNextFileData, bool>
    {
        private readonly IBus _bus;
        private static readonly IDictionary<Guid, FileDownloader> Downloaders = new ConcurrentDictionary<Guid, FileDownloader>();

        public UploadHandler(IBus bus)
        {
            _bus = bus;
        }

        public FileDownloaderStarted Handle(StartFileUpload command)
        {
            var newId = Guid.NewGuid();
            Downloaders.Add(newId, new FileDownloader(command.DestinationFilePath, command.Length));
            return new FileDownloaderStarted {Id = newId};
        }

        public bool Handle(SendNextFileData command)
        {
            FileDownloader downloader;
            if (Downloaders.TryGetValue(command.Id, out downloader) == false) {
                throw new Exception($"Unable to use the file data, downloader with id '{command.Id}' does not exist.");
            }
            downloader.AppendData(command.Buffer);
            if (downloader.IsFinished()) {
                _bus.Send(new FileDownloaderEnded {Id = command.Id});
                Downloaders.Remove(command.Id);
            }
            return true;
        }
    }

    public class FileDownloader
    {
        private readonly string _destinationFilePath;
        private readonly long _length;

        public FileDownloader(string destinationFilePath, long length)
        {
            _destinationFilePath = destinationFilePath;
            _length = length;

            if (File.Exists(destinationFilePath)) {
                File.Delete(destinationFilePath);
            }
        }

        public void AppendData(byte[] buffer)
        {
            using (var stream = File.Open(_destinationFilePath, FileMode.Append)) {
                stream.Write(buffer, 0, buffer.Length);
            }
        }

        public bool IsFinished()
        {
            return new FileInfo(_destinationFilePath).Length == _length;
        }
    }
}