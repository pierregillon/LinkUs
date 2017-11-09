using System;
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
        public Task Receive<TResponse>(ClientId @from, Predicate<TResponse> predicate)
        {
            @from = @from ?? ClientId.Server;
            var completionSource = new TaskCompletionSource<TResponse>();

            EventHandler<Package> packageReceivedAction = (sender, package) => {
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
                PackageTransmitter.PackageReceived -= packageReceivedAction;
                return task.Result;
            });
        }
    }
}