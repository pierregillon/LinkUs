using System;

namespace LinkUs.Client.FileTransfert {
    public interface IFilePointer
    {
        Guid FileId { get; set; }
    }
}