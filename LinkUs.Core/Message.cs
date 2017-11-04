namespace LinkUs.Core
{
    public abstract class Message
    {
        public string Name => GetType().Name;
    }
}