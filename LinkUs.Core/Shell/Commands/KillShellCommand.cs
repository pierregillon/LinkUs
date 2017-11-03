namespace LinkUs.Core.Shell.Commands
{
    public class KillShellCommand : ShellMessage
    {
        public KillShellCommand() { }
        public KillShellCommand(double processId) : base(processId)
        {
        }
    }
}