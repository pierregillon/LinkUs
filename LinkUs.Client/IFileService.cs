namespace LinkUs.Client
{
    public interface IFileService
    {
        bool Exists(string filePath);
        string GetRandomFileName();
        string GetFileNameCopiedFromExisting(string directoryPath);
        void Copy(string source, string target);
    }
}