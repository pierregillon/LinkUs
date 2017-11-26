using System;

namespace LinkUs.Client.FileTransfert.Commands
{
    public class GetNextFileData : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}