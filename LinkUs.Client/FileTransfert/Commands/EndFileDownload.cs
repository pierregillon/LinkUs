using System;

namespace LinkUs.Client.FileTransfert.Commands
{
    public class EndFileDownload : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}