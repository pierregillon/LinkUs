using System;

namespace LinkUs.Modules.Default.FileTransfert.Commands
{
    public class SendNextFileData : IFilePointer
    {
        public Guid FileId { get; set; }
        public byte[] Buffer { get; set; }
    }
}