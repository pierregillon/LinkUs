using System;

namespace LinkUs.Modules.Default.FileTransfert.Events
{
    public class FileDownloadEnded: IFilePointer
    {
        public Guid FileId { get; set; }
    }
}