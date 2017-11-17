using System;

namespace LinkUs.Modules.Default.FileTransfert.Events
{
    public class FileUploadStarted : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}