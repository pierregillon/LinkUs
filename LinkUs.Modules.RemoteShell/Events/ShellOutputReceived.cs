namespace LinkUs.Modules.RemoteShell.Events
{
    public class ShellOutputReceived : ShellMessage
    {
        public string Output { get; set; }

        public ShellOutputReceived() { }
        public ShellOutputReceived(string output, int processId) : base(processId)
        {
            Output = output;
        }
    }
}