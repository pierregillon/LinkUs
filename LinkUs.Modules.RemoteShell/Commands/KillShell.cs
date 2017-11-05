namespace LinkUs.Modules.RemoteShell.Commands
{
    public class KillShell : ShellMessage
    {
        public KillShell() { }
        public KillShell(double processId) : base(processId) { }
    }
}