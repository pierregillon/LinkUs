using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;

namespace LinkUs.Core
{
    public class CommandSender : ICommandSender
    {
        private readonly ISerializer _serializer;
        public PackageTransmitter PackageTransmitter { get; }

        public CommandSender(PackageTransmitter packageTransmitter, ISerializer serializer)
        {
            PackageTransmitter = packageTransmitter;
            _serializer = serializer;
        }

        public Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command, ClientId destination = null, ClientId source = null)
        {
            destination = destination ?? ClientId.Server;
            source = source ?? ClientId.Unknown;

            var commandPackage = new Package(source, destination, _serializer.Serialize(command));

            var completionSource = new TaskCompletionSource<TResponse>();
            EventHandler<Package> packageReceivedAction = (sender, responsePackage) => {
                if (Equals(responsePackage.TransactionId, commandPackage.TransactionId)) {
                    try {
                        var response = _serializer.Deserialize<TResponse>(responsePackage.Content);
                        completionSource.SetResult(response);
                    }
                    catch (Exception ex) {
                        completionSource.SetException(ex);
                    }
                }
            };
            EventHandler closedAction = (sender, args) => {
                completionSource.SetException(new Exception("Connection Closed"));
            };
            PackageTransmitter.PackageReceived += packageReceivedAction;
            PackageTransmitter.Closed += closedAction;
            PackageTransmitter.Send(commandPackage);

            return completionSource.Task.ContinueWith(task => {
                PackageTransmitter.PackageReceived -= packageReceivedAction;
                PackageTransmitter.Closed -= closedAction;
                return task.Result;
            });
        }
        public void ExecuteAsync<TCommand>(TCommand command, ClientId destination = null)
        {
            destination = destination ?? ClientId.Server;
            var content = _serializer.Serialize(command);
            var commandPackage = new Package(ClientId.Unknown, destination, content);
            PackageTransmitter.Send(commandPackage);
        }
        public Task<TResponse> Receive<TResponse>(ClientId @from, Predicate<TResponse> predicate)
        {
            @from = @from ?? ClientId.Server;
            var completionSource = new TaskCompletionSource<TResponse>();

            EventHandler<Package> packageReceivedAction = null;
            packageReceivedAction = (sender, package) => {
                if (!Equals(package.Source, @from)) {
                    return;
                }
                if (_serializer.IsPrimitifMessage(package.Content)) {
                    return;
                }
                try {
                    var messageDescriptor = _serializer.Deserialize<MessageDescriptor>(package.Content);
                    if (messageDescriptor.CommandName == typeof(TResponse).Name) {
                        var response = _serializer.Deserialize<TResponse>(package.Content);
                        if (predicate(response)) {
                            completionSource.SetResult(response);
                        }
                    }
                }
                catch (Exception ex) {
                    completionSource.SetException(ex);
                }
            };

            PackageTransmitter.PackageReceived += packageReceivedAction;

            return completionSource.Task.ContinueWith(task => {
                return task.Result;
            });
        }
        public void AnswerAsync<T>(T message, Package package)
        {
            var content = _serializer.Serialize(message);
            var commandPackage = package.CreateResponsePackage(content);
            PackageTransmitter.Send(commandPackage);
        }
        public CommandStream<T> BuildStream<T>(Predicate<T> predicate)
        {
            return new CommandStream<T>(PackageTransmitter, _serializer);
        }
    }

    public class CommandStream<T>
    {
        private readonly PackageTransmitter _transmitter;
        private readonly ISerializer _serializer;
        private readonly ConcurrentQueue<T> _values = new ConcurrentQueue<T>();
        private bool _ended;

        public CommandStream(PackageTransmitter transmitter, ISerializer serializer)
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
        }
        public void End()
        {
            _transmitter.PackageReceived -= TransmitterOnPackageReceived;
            _ended = true;
        }

        private void TransmitterOnPackageReceived(object o, Package package)
        {
            if (_serializer.IsPrimitifMessage(package.Content)) {
                return;
            }
            var messageDescriptor = _serializer.Deserialize<MessageDescriptor>(package.Content);
            if (messageDescriptor.CommandName == typeof(T).Name) {
                var response = _serializer.Deserialize<T>(package.Content);
                _values.Enqueue(response);
            }
        }
    }
}