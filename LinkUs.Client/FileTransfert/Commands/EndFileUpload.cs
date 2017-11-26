using System;

namespace LinkUs.Client.FileTransfert.Commands
{
    public class EndFileUpload : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}