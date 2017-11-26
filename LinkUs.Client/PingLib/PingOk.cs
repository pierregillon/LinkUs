namespace LinkUs.Client.PingLib
{
    public class PingOk 
    {
        public string Message { get; set; }
        public PingOk() { }
        public PingOk(string message)
        {
            Message = message;
        }
    }
}