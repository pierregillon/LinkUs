namespace LinkUs.Core
{
    public class PingHandler : IHandler<Ping, PingOk>
    {
        public PingOk Handle(Ping ping)
        {
            return new PingOk("OK");
        }
    }
}