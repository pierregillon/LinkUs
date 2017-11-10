namespace LinkUs.Core
{
    public interface IBus
    {
        void Answer<TCommand>(TCommand message);
        void Send<TCommand>(TCommand message);
        TResponse Send<TCommand, TResponse>(TCommand command);
    }
}