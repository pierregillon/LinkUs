using System;

namespace LinkUs.Core.FileTransfert.Events
{
    public class FileDownloadStarted: IFilePointer
    {
        public Guid FileId { get; set; }
        public long TotalLength { get; set; }
    }
}