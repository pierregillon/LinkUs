namespace LinkUs.Modules.RemoteShell
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