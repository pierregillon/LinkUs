using System;

namespace LinkUs.Modules.Default.FileTransfert {
    public interface IFilePointer
    {
        Guid FileId { get; set; }
    }
}