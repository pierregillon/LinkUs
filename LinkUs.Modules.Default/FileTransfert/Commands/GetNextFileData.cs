using System;

namespace LinkUs.Modules.Default.FileTransfert.Commands
{
    public class GetNextFileData : IFilePointer
    {
        public Guid FileId { get; set; }
    }
}