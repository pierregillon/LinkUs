namespace LinkUs.Modules.RemoteShell
{
    public abstract class ShellMessage 
    {
        public double ProcessId { get; set; }

        protected ShellMessage() { }
        protected ShellMessage(double processId)
        {
            ProcessId = processId;
        }
    }
}