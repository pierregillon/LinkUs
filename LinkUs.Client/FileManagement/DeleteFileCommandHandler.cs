using System.IO;
using LinkUs.Core.Commands;

namespace LinkUs.Client.FileManagement
{
    public class DeleteFileCommandHandler : ICommandHandler<DeleteFileCommand, bool>
    {
        public bool Handle(DeleteFileCommand command)
        {
            File.Delete(command.FilePath);
            return true;
        }
    }
}