using System;

namespace LinkUs.Core.FileTransfert.Commands
{
    public class EndFileUpload : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}