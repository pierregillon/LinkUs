namespace LinkUs.Core.Shell
{
    public class ShellOuputReceivedResponse : Command
    {
        public ShellOuputReceivedResponse() : base("ShellOuputReceivedResponse") { }
        public ShellOuputReceivedResponse(string output, double processId) : this()
        {
            Output = output;
            ProcessId = processId;
        }

        public string Output { get; set; }
        public double ProcessId { get; set; }
    }
}