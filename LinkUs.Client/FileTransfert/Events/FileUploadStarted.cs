using System;

namespace LinkUs.Client.FileTransfert.Events
{
    public class FileUploadStarted : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}