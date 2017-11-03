namespace LinkUs.Core.Shell
{
    public class ShellEndedResponse : Command
    {
        public ShellEndedResponse() : base("ShellEndedResponse") { }
        public ShellEndedResponse(double exitCode, double processId) : this()
        {
            ExitCode = exitCode;
            ProcessId = processId;
        }
        public double ExitCode { get; set; }
        public double ProcessId { get; set; }
    }
}