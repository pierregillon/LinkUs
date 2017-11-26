using System;

namespace LinkUs.Client.FileTransfert.Events
{
    public class FileDownloadStarted: IFilePointer
    {
        public Guid FileId { get; set; }
        public long TotalLength { get; set; }
    }
}