namespace LinkUs.Client {
    public interface IEnvironment
    {
        string ApplicationPath { get; }
        string InstallationDirectory { get; }
    }
}