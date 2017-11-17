using System;
using System.Threading;
using LinkUs.Core.Connection;
using LinkUs.Core.Packages;

namespace LinkUs.Client
{
    public class RequestProcessor
    {
        private readonly PackageTransmitter _packageTransmitter;
        private readonly PackageProcessor _packageProcessor;
        private readonly ManualResetEvent _manualResetEvent = new ManualResetEvent(false);

        public RequestProcessor(
            PackageTransmitter packageTransmitter,
            PackageProcessor packageProcessor)
        {
            _packageTransmitter = packageTransmitter;
            _packageProcessor = packageProcessor;
        }

        public void ProcessRequests()
        {
            _packageTransmitter.PackageReceived += (sender, package) => {
                Console.WriteLine(package);
                _packageProcessor.Process(package);
            };
            _packageTransmitter.Closed += (sender, eventArgs) => {
                _manualResetEvent.Set();
            };
            _manualResetEvent.WaitOne();
            _packageTransmitter.Close();
        }
    }
}