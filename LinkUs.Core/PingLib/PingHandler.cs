namespace LinkUs.Core.PingLib
{
    public class PingHandler : IHandler<Ping, PingOk>
    {
        public PingOk Handle(Ping command)
        {
            return new PingOk("OK");
        }
    }
}