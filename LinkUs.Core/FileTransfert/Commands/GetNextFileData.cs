using System;

namespace LinkUs.Core.FileTransfert.Commands
{
    public class GetNextFileData : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}