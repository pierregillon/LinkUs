namespace LinkUs.Core.Shell.Events
{
    public class ShellOutputReceived : ShellMessage
    {
        public string Output { get; set; }

        public ShellOutputReceived() { }
        public ShellOutputReceived(string output, double processId) : base(processId)
        {
            Output = output;
        }
    }
}