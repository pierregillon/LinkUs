namespace LinkUs.Client
{
    public interface IProcessManager
    {
        bool StartProcessWithElevatedPrivileges(string exeFilePath);
    }
}