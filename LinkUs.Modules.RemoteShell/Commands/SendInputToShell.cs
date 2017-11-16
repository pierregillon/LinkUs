namespace LinkUs.Modules.RemoteShell.Commands
{
    public class SendInputToShell : ShellMessage
    {
        public string Input { get; set; }

        public SendInputToShell() { }
        public SendInputToShell(string input, int processId) : base(processId)
        {
            Input = input;
        }
    }
}