using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using LinkUs.Core.Commands;
using LinkUs.Modules.Default.FileTransfert.Commands;
using LinkUs.Modules.Default.FileTransfert.Events;

namespace LinkUs.Modules.Default.FileTransfert
{
    public class UploadFileCommandHandler :
        ICommandHandler<StartFileUpload, FileUploadStarted>,
        ICommandHandler<SendNextFileData, bool>,
        ICommandHandler<EndFileUpload, FileUploadEnded>
    {
        private static readonly IDictionary<Guid, Stream> OpenedStreams = new ConcurrentDictionary<Guid, Stream>();

        // ----- Public methods
        public FileUploadStarted Handle(StartFileUpload command)
        {
            try {
                if (File.Exists(command.DestinationFilePath)) {
                    File.Delete(command.DestinationFilePath);
                }
                var newId = Guid.NewGuid();
                OpenedStreams.Add(newId, File.Open(command.DestinationFilePath, FileMode.Append));
                return new FileUploadStarted { FileId = newId };
            }
            catch (Exception ex) {
                throw new Exception($"Unable to start the upload of the file '{command.DestinationFilePath}'.", ex);
            }
        }
        public bool Handle(SendNextFileData command)
        {
            var fileStream = GetOpenedFileStream(command.FileId);
            try {
                fileStream.Write(command.Buffer, 0, command.Buffer.Length);
                return true;
            }
            catch (Exception ex) {
                fileStream.Close();
                fileStream.Dispose();
                OpenedStreams.Remove(command.FileId);
                throw new Exception("An unexpected error occurred during the file upload.", ex);
            }
        }
        public FileUploadEnded Handle(EndFileUpload command)
        {
            Stream fileStream;
            if (OpenedStreams.TryGetValue(command.FileId, out fileStream)) {
                fileStream.Close();
                fileStream.Dispose();
                OpenedStreams.Remove(command.FileId);
            }
            return new FileUploadEnded { FileId = command.FileId };
        }

        // ----- Utils
        private static Stream GetOpenedFileStream(Guid fileId)
        {
            Stream fileStream;
            if (OpenedStreams.TryGetValue(fileId, out fileStream) == false) {
                throw new Exception($"Unable to upload, the file id '{fileId}' does not exist.");
            }
            return fileStream;
        }
    }
}