using System;

namespace LinkUs.Client.FileTransfert.Events
{
    public class FileUploadEnded : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}