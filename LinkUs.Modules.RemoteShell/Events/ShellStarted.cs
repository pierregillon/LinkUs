namespace LinkUs.Modules.RemoteShell.Events
{
    public class ShellStarted : ShellMessage
    {
        public ShellStarted() { }
        public ShellStarted(double processId) : base(processId) { }
    }
}