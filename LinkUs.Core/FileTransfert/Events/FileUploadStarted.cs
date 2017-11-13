using System;

namespace LinkUs.Core.FileTransfert.Events
{
    public class FileUploadStarted : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}