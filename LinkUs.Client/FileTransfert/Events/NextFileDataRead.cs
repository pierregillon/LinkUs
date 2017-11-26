using System;

namespace LinkUs.Client.FileTransfert.Events
{
    public class NextFileDataRead : IFilePointer
    {
        public Guid FileId { get; set; }
        public byte[] Data { get; set; }
    }
}