using System;

namespace LinkUs.Core.FileTransfert.Events
{
    public class FileUploadEnded : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}