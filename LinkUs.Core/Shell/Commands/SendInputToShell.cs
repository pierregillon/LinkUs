namespace LinkUs.Core.Shell.Commands
{
    public class SendInputToShell : ShellMessage
    {
        public string Input { get; set; }

        public SendInputToShell() { }
        public SendInputToShell(string input, double processId) : base(processId)
        {
            Input = input;
        }
    }
}