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
        IHandler<SendNextFileData, bool>,
        IHandler<EndFileUpload, FileDownloaderEnded>
    {
        private static readonly IDictionary<Guid, Stream> FileStreams = new ConcurrentDictionary<Guid, Stream>();

        public FileDownloaderStarted Handle(StartFileUpload command)
        {
            if (File.Exists(command.DestinationFilePath)) {
                File.Delete(command.DestinationFilePath);
            }
            var newId = Guid.NewGuid();
            FileStreams.Add(newId, File.Open(command.DestinationFilePath, FileMode.Append));
            return new FileDownloaderStarted {Id = newId};
        }

        public bool Handle(SendNextFileData command)
        {
            var fileStream = GetFileStream(command.Id);
            try {
                fileStream.Write(command.Buffer, 0, command.Buffer.Length);
                return true;
            }
            catch (Exception) {
                fileStream.Close();
                fileStream.Dispose();
                FileStreams.Remove(command.Id);
                throw;
            }
        }

        public FileDownloaderEnded Handle(EndFileUpload command)
        {
            Stream fileStream;
            if (FileStreams.TryGetValue(command.Id, out fileStream)) {
                fileStream.Close();
                fileStream.Dispose();
                FileStreams.Remove(command.Id);
            }
            return new FileDownloaderEnded {Id = command.Id};
        }

        private static Stream GetFileStream(Guid id)
        {
            Stream fileStream;
            if (FileStreams.TryGetValue(id, out fileStream) == false) {
                throw new Exception($"Unable to use the file data, downloader with id '{id}' does not exist.");
            }
            return fileStream;
        }
    }
}