namespace LinkUs.Core.Shell
{
    public abstract class ShellMessage : Message
    {
        public double ProcessId { get; set; }

        protected ShellMessage() { }
        protected ShellMessage(double processId)
        {
            ProcessId = processId;
        }
    }
}