using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using LinkUs.Core.FileTransfert.Commands;
using LinkUs.Core.FileTransfert.Events;

namespace LinkUs.Core.FileTransfert
{
    public class UploadFileCommandHandler :
        IHandler<StartFileUpload, FileUploadStarted>,
        IHandler<SendNextFileData, bool>,
        IHandler<EndFileUpload, FileUploadEnded>
    {
        private static readonly IDictionary<Guid, Stream> FileStreams = new ConcurrentDictionary<Guid, Stream>();

        public FileUploadStarted Handle(StartFileUpload command)
        {
            if (File.Exists(command.DestinationFilePath)) {
                File.Delete(command.DestinationFilePath);
            }
            var newId = Guid.NewGuid();
            FileStreams.Add(newId, File.Open(command.DestinationFilePath, FileMode.Append));
            return new FileUploadStarted {FileId = newId};
        }

        public bool Handle(SendNextFileData command)
        {
            var fileStream = GetFileStream(command.FileId);
            try {
                fileStream.Write(command.Buffer, 0, command.Buffer.Length);
                return true;
            }
            catch (Exception) {
                fileStream.Close();
                fileStream.Dispose();
                FileStreams.Remove(command.FileId);
                throw;
            }
        }

        public FileUploadEnded Handle(EndFileUpload command)
        {
            Stream fileStream;
            if (FileStreams.TryGetValue(command.FileId, out fileStream)) {
                fileStream.Close();
                fileStream.Dispose();
                FileStreams.Remove(command.FileId);
            }
            return new FileUploadEnded {FileId = command.FileId};
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