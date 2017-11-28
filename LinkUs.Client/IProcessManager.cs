namespace LinkUs.Client
{
    public interface IProcessManager
    {
        bool TryStartProcessWithElevatedPrivileges(string exeFilePath);
        void StartProcessWithElevatedPrivileges(string exeFilePath);
        void StartProcessWithCurrentPrivileges(string exeFilePath);
        void StartProcess(string exeFilePath);
        bool IsProcessStarted(string fileName);
    }
}