using System;

namespace LinkUs.Modules.Default.FileTransfert.Events
{
    public class NextFileDataRead : IFilePointer
    {
        public Guid FileId { get; set; }
        public byte[] Data { get; set; }
    }
}