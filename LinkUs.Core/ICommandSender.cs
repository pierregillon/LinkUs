using System;
using System.Threading;
using System.Threading.Tasks;
using LinkUs.Core.Connection;

namespace LinkUs.Core
{
    public interface ICommandSender
    {
        Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command, ClientId destination = null, ClientId source = null);
        void ExecuteAsync<TCommand>(TCommand command, ClientId destination = null);
        Task<T> Receive<T>(ClientId @from, Predicate<T> predicate, CancellationToken token);
        void AnswerAsync<T>(T message, Package package);
        CommandStream<T> BuildStream<T>(Predicate<T> predicate);
    }
}