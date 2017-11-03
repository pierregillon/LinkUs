namespace LinkUs.Core
{
    public class PingOk : Message
    {
        public string Message { get; set; }
        public PingOk() { }
        public PingOk(string message)
        {
            Message = message;
        }
    }
}