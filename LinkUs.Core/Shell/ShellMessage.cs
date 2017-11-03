namespace LinkUs.Core.Shell
{
    public class ShellMessage : Message
    {
        public double ProcessId { get; set; }

        public ShellMessage() { }
        public ShellMessage(double processId)
        {
            ProcessId = processId;
        }
    }
}