namespace LinkUs.Core.Shell
{
    public class SendInputToShellCommand : Command
    {
        public SendInputToShellCommand() : base("SendInputToShellCommand") { }
        public SendInputToShellCommand(string input, double processId) : this()
        {
            Input = input;
            ProcessId = processId;
        }

        public string Input { get; set; }
        public double ProcessId { get; set; }
    }
}