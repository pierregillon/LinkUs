namespace LinkUs.Client
{
    public interface IRegistry
    {
        bool IsRegisteredAtStartup(string filePath);
        void AddFileToStartupRegistry(string filePath);
        void RemoveFileFromStartupRegistry(string filePath);

        void SetFileLocation(string filePath);
        void ClearFileLocation();
        string GetFileLocation();
    }
}