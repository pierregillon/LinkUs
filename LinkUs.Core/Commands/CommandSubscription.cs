using System;
using LinkUs.Core.Packages;

namespace LinkUs.Core.Commands
{
    public class CommandSubscription : IDisposable
    {
        private PackageTransmitter _packageTransmitter;
        private EventHandler<Package> _subscription;

        public CommandSubscription(PackageTransmitter packageTransmitter, EventHandler<Package> subscription)
        {
            _packageTransmitter = packageTransmitter;
            _subscription = subscription;
        }
        public void Dispose()
        {
            _packageTransmitter.PackageReceived -= _subscription;
            _packageTransmitter = null;
            _subscription = null;
        }
    }
}