using System;

namespace LinkUs.Core.FileTransfert.Commands
{
    public class FileUploaderStarted
    {
        public Guid Id { get; set; }
        public long TotalLength { get; set; }
    }
}