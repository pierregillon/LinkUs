namespace LinkUs.Core.Shell.Commands
{
    public class SendInputToShellCommand : ShellMessage
    {
        public string Input { get; set; }

        public SendInputToShellCommand() { }
        public SendInputToShellCommand(string input, double processId) : base(processId)
        {
            Input = input;
        }
    }
}