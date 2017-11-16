using System;

namespace LinkUs.Core.FileTransfert.Commands
{
    public class EndFileDownload : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}