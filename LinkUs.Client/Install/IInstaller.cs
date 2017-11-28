namespace LinkUs.Client.Install
{
    public interface IInstaller
    {
        string Install(string exeFile);
        void Uninstall(string exeFile);
        bool IsInstalled(string exeFile);
        void CheckInstall(string exeFile);
    }
}