using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;

namespace LinkUs.Client
{
    public class DedicatedBus : IBus
    {
        private readonly PackageTransmitter _transmitter;
        private readonly ClientId _target;
        private readonly ISerializer _serializer;

        public DedicatedBus(PackageTransmitter transmitter, ClientId target, ISerializer serializer)
        {
            _transmitter = transmitter;
            _target = target;
            _serializer = serializer;
        }

        public void Send(object message)
        {
            var data = _serializer.Serialize(message);
            var package = new Package(ClientId.Unknown, _target, data);
            _transmitter.Send(package);
        }
    }
}