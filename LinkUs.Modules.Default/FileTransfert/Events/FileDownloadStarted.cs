using System;

namespace LinkUs.Modules.Default.FileTransfert.Events
{
    public class FileDownloadStarted: IFilePointer
    {
        public Guid FileId { get; set; }
        public long TotalLength { get; set; }
    }
}