namespace LinkUs.Core.Shell
{
    public class KillShellCommand : Command
    {
        public KillShellCommand() : base("KillShellCommand") { }
        public KillShellCommand(double processId) : this()
        {
            ProcessId = processId;
        }
        public double ProcessId { get; set; }
    }
}