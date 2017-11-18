using System;
using System.Threading;
using System.Threading.Tasks;
using LinkUs.Core.Packages;

namespace LinkUs.Core.Commands
{
    public interface ICommandSender
    {
        Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command, ClientId destination = null, ClientId source = null);
        void ExecuteAsync<TCommand>(TCommand command, ClientId destination = null);
        Task<T> Receive<T>(ClientId @from, Predicate<T> predicate, CancellationToken token);
        void AnswerAsync<T>(T message, Package package);
        CommandStream<T> BuildStream<T>(Predicate<T> predicate);
        CommandSubscription Subscribe<T>(Action<T> callback, Predicate<T> predicate);
    }
}