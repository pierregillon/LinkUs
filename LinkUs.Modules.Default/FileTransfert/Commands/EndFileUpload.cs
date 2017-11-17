using System;

namespace LinkUs.Modules.Default.FileTransfert.Commands
{
    public class EndFileUpload : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}