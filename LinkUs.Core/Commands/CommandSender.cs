using System;
using System.Threading;
using System.Threading.Tasks;
using LinkUs.Core.Json;
using LinkUs.Core.Packages;

namespace LinkUs.Core.Commands
{
    public class CommandSender : ICommandSender
    {
        private readonly ICommandSerializer _serializer;
        private readonly PackageTransmitter _packageTransmitter;

        // ----- Constructors
        public CommandSender(PackageTransmitter packageTransmitter, ICommandSerializer serializer)
        {
            _packageTransmitter = packageTransmitter;
            _serializer = serializer;
        }

        // ----- Public methods
        public Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command, ClientId destination = null, ClientId source = null)
        {
            var tokenSource = new CancellationTokenSource();
            var timeout = TimeSpan.FromSeconds(5);
            var delayBeforeTimeoutTask = Task.Delay(timeout, tokenSource.Token);
            var sendCommandTask = Send<TCommand, TResponse>(command, destination, source, tokenSource.Token);

            return Task.WhenAny(sendCommandTask, delayBeforeTimeoutTask)
                       .ContinueWith(t => {
                           try {
                               if (!sendCommandTask.IsCompleted) {
                                   tokenSource.Cancel();
                                   throw new ExecuteCommandTimeoutException(timeout);
                               }
                               else {
                                   return sendCommandTask.Result;
                               }
                           }
                           finally {
                               tokenSource.Dispose();
                           }
                       }, tokenSource.Token);
        }
        private Task<TResponse> Send<TCommand, TResponse>(TCommand command, ClientId destination, ClientId source, CancellationToken token)
        {
            destination = destination ?? ClientId.Server;
            source = source ?? ClientId.Unknown;

            var commandPackage = new Package(source, destination, _serializer.Serialize(command));
            var completionSource = new TaskCompletionSource<TResponse>();

            EventHandler <Package> packageReceivedAction = (sender, responsePackage) => {
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

            var registration = token.Register(() => {
                completionSource.SetCanceled();
                _packageTransmitter.PackageReceived -= packageReceivedAction;
                _packageTransmitter.Closed -= closedAction;
            }, true);

            _packageTransmitter.PackageReceived += packageReceivedAction;
            _packageTransmitter.Closed += closedAction;
            _packageTransmitter.Send(commandPackage);

            return completionSource.Task.ContinueWith(task => {
                _packageTransmitter.PackageReceived -= packageReceivedAction;
                _packageTransmitter.Closed -= closedAction;
                registration.Dispose();
                return task.Result;
            }, token);
        }
        public void ExecuteAsync<TCommand>(TCommand command, ClientId destination = null)
        {
            destination = destination ?? ClientId.Server;
            var content = _serializer.Serialize(command);
            var commandPackage = new Package(ClientId.Unknown, destination, content);
            _packageTransmitter.Send(commandPackage);
        }
        public Task<TResponse> Receive<TResponse>(ClientId @from, Predicate<TResponse> predicate, CancellationToken token)
        {
            @from = @from ?? ClientId.Server;
            var completionSource = new TaskCompletionSource<TResponse>();

            EventHandler<Package> packageReceivedAction = null;
            packageReceivedAction = (sender, package) => {
                if (token.IsCancellationRequested) {
                    _packageTransmitter.PackageReceived -= packageReceivedAction;
                    completionSource.SetCanceled();
                    return;
                }
                if (!Equals(package.Source, @from)) {
                    return;
                }
                if (_serializer.IsPrimitifMessage(package.Content)) {
                    return;
                }
                try {
                    var messageDescriptor = _serializer.Deserialize<CommandDescriptor>(package.Content);
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

            _packageTransmitter.PackageReceived += packageReceivedAction;

            return completionSource.Task.ContinueWith(task => {
                _packageTransmitter.PackageReceived -= packageReceivedAction;
                return task.Result;
            }, token);
        }
        public void AnswerAsync<T>(T message, Package package)
        {
            var content = _serializer.Serialize(message);
            var commandPackage = package.CreateResponsePackage(content);
            _packageTransmitter.Send(commandPackage);
        }
        public CommandStream<T> BuildStream<T>(Predicate<T> predicate)
        {
            return new CommandStream<T>(_packageTransmitter, _serializer);
        }
    }

    public sealed class CancellationTokenTaskSource<T> : IDisposable
    {
        private readonly IDisposable _registration;

        public CancellationTokenTaskSource(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<T>();
            _registration = cancellationToken.Register(() => tcs.TrySetCanceled(), false);
            Task = tcs.Task;
        }

        public Task<T> Task { get; }
        public void Dispose()
        {
            _registration?.Dispose();
        }
    }
}