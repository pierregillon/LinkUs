namespace LinkUs.Core.Shell.Events
{
    public class ShellStarted : ShellMessage
    {
        public ShellStarted() { }
        public ShellStarted(double processId) : base(processId) { }
    }
}