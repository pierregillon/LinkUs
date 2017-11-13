using System;

namespace LinkUs.Core.FileTransfert.Commands
{
    public class SendNextFileData : IFilePointer
    {
        public Guid FileId { get; set; }
        public byte[] Buffer { get; set; }
    }
}