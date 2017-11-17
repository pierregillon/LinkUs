using System;

namespace LinkUs.Modules.Default.FileTransfert.Events
{
    public class FileUploadEnded : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}