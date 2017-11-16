namespace LinkUs.Modules.RemoteShell
{
    public abstract class ShellMessage 
    {
        public int ProcessId { get; set; }

        protected ShellMessage() { }
        protected ShellMessage(int processId)
        {
            ProcessId = processId;
        }
    }
}