namespace LinkUs.Client
{
    public interface IProcessManager
    {
        void StartProcess(string exeFilePath);
        bool IsProcessStarted(string fileName);
    }
}