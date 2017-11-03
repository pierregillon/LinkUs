namespace LinkUs.Core.Shell.Commands
{
    public class KillShell : ShellMessage
    {
        public KillShell() { }
        public KillShell(double processId) : base(processId) { }
    }
}