using System;

namespace LinkUs.Core.FileTransfert.Commands
{
    public class SendNextFileData
    {
        public Guid Id { get; set; }
        public byte[] Buffer { get; set; }
    }
}