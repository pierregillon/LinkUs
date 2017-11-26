namespace LinkUs.Modules.Default.Install {
    public interface IEnvironment
    {
        string ApplicationPath { get; }
        string InstallationDirectory { get; }
    }
}