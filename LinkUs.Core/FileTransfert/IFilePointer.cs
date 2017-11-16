using System;

namespace LinkUs.Core.FileTransfert {
    public interface IFilePointer
    {
        Guid FileId { get; set; }
    }
}