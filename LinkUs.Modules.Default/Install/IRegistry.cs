namespace LinkUs.Modules.Default.Install
{
    public interface IRegistry
    {
        bool IsRegisteredAtStartup(string filePath);
        void Add(string filePathLocationRegistry, string key, string value);
        void AddFileToStartupRegistry(string filePath);
        void RemoveFileFromStartupRegistry(string filePath);
        void Remove(string registry, string key);
        string Get(string registry, string key);
    }
}