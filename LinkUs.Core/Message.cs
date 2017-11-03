namespace LinkUs.Core
{
    public class Message
    {
        public string Name { get; set; }

        public Message()
        {
            Name = this.GetType().Name;
        }
    }
}