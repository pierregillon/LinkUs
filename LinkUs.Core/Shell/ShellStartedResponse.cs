namespace LinkUs.Core.Shell
{
    public class ShellStartedResponse : Command
    {
        public ShellStartedResponse() : base("ShellStartedResponse") { }
        public ShellStartedResponse(double processId)
        {
            ProcessId = processId;
        }
        public double ProcessId { get; set; }
    }
}