namespace LinkUs.Core
{
    public interface IHandler<in TRequest, out TResponse>
    {
        TResponse Handle(TRequest request);
    }

    public interface IHandler<in TRequest>
    {
        void Handle(TRequest command);
    }
}