using System;

namespace LinkUs.Core.FileTransfert.Events
{
    public class FileDownloadEnded: IFilePointer
    {
        public Guid FileId { get; set; }
    }
}