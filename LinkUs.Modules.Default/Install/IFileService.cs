namespace LinkUs.Modules.Default.Install
{
    public interface IFileService
    {
        bool Exists(string filePath);
        string GetRandomFileName();
        string GetFileNameCopiedFromExisting(string directoryPath);
        void Copy(string source, string target);
    }
}