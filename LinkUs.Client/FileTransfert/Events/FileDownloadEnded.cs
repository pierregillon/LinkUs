using System;

namespace LinkUs.Client.FileTransfert.Events
{
    public class FileDownloadEnded: IFilePointer
    {
        public Guid FileId { get; set; }
    }
}