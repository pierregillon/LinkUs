namespace LinkUs.Modules.RemoteShell.Events
{
    public class ShellStarted : ShellMessage
    {
        public ShellStarted() { }
        public ShellStarted(int processId) : base(processId) { }
    }
}