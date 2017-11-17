using System;

namespace LinkUs.Modules.Default.FileTransfert.Commands
{
    public class EndFileDownload : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}