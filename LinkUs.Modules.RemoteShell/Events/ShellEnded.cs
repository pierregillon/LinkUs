namespace LinkUs.Modules.RemoteShell.Events
{
    public class ShellEnded : ShellMessage
    {
        public double ExitCode { get; set; }

        public ShellEnded() { }
        public ShellEnded(double exitCode, double processId) : base(processId)
        {
            ExitCode = exitCode;
        }
    }
}