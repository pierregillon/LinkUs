using System;

namespace LinkUs.Client.FileTransfert.Commands
{
    public class SendNextFileData : IFilePointer
    {
        public Guid FileId { get; set; }
        public byte[] Buffer { get; set; }
    }
}