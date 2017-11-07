namespace LinkUs.Modules.RemoteShell.Commands
{
    public class KillShell : ShellMessage
    {
        public KillShell() { }
        public KillShell(int processId) : base(processId) { }
    }
}