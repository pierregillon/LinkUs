namespace LinkUs.Client
{
    public interface IRegistry
    {
        bool IsRegisteredAtStartup(string filePath);
        void AddFileToStartupRegistry(string filePath);
        void RemoveFileFromStartupRegistry(string filePath);

        string Get(string registry, string key);
        void Set(string filePathLocationRegistry, string key, string value);
        void Remove(string registry, string key);
    }
}