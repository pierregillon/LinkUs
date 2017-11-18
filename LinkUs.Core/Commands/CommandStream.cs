using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using LinkUs.Core.Packages;

namespace LinkUs.Core.Commands
{
    public class CommandStream<T>
    {
        private readonly PackageTransmitter _transmitter;
        private readonly ICommandSerializer _serializer;
        private readonly ConcurrentQueue<T> _values = new ConcurrentQueue<T>();
        private bool _ended;
        private Exception _lastError;

        public CommandStream(PackageTransmitter transmitter, ICommandSerializer serializer)
        {
            _transmitter = transmitter;
            _serializer = serializer;
        }

        public void Start()
        {
            _transmitter.PackageReceived += TransmitterOnPackageReceived;
        }
        public IEnumerable<T> GetData()
        {
            while (!_ended || _values.Count != 0) {
                T value;
                if (_values.TryDequeue(out value)) {
                    yield return value;
                }
            }

            if (_lastError != null) {
                throw _lastError;
            }
        }
        public void End()
        {
            _transmitter.PackageReceived -= TransmitterOnPackageReceived;
            _ended = true;
        }

        private void TransmitterOnPackageReceived(object o, Package package)
        {
            try {
                if (_serializer.IsPrimitifMessage(package.Content)) {
                    return;
                }
                var messageDescriptor = _serializer.Deserialize<CommandDescriptor>(package.Content);
                if (messageDescriptor.CommandName == typeof(T).Name) {
                    var response = _serializer.Deserialize<T>(package.Content);
                    _values.Enqueue(response);
                }
            }
            catch (Exception ex) {
                _lastError = ex;
                End();
            }
        }
    }
}