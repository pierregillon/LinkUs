namespace LinkUs.Modules.RemoteShell.Events
{
    public class ShellEnded : ShellMessage
    {
        public int ExitCode { get; set; }

        public ShellEnded() { }
        public ShellEnded(int exitCode, int processId) : base(processId)
        {
            ExitCode = exitCode;
        }
    }
}