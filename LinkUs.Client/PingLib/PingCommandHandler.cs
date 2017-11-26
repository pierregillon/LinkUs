using LinkUs.Core.Commands;

namespace LinkUs.Client.PingLib
{
    public class PingCommandHandler : ICommandHandler<Ping, PingOk>
    {
        public PingOk Handle(Ping command)
        {
            return new PingOk("OK");
        }
    }
}