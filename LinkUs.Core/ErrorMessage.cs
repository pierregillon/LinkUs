namespace LinkUs.Core
{
    public class ErrorMessage : Message
    {
        public string Error { get; set; }

        public ErrorMessage() { }
        public ErrorMessage(string error)
        {
            Error = error;
        }
    }
}