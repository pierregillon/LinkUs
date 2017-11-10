using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;

namespace LinkUs.Client
{
    public class DedicatedBus : IBus
    {
        private readonly ICommandSender _commandSender;
        private readonly Package _package;

        public DedicatedBus(ICommandSender commandSender, Package package)
        {
            _commandSender = commandSender;
            _package = package;
        }

        public void Answer<TCommand>(TCommand message)
        {
            _commandSender.AnswerAsync(message, _package);
        }
        public void Send<TCommand>(TCommand message)
        {
            _commandSender.ExecuteAsync(message, _package.Source);
        }
        public TResponse Send<TCommand, TResponse>(TCommand command)
        {
            return _commandSender.ExecuteAsync<TCommand, TResponse>(command, _package.Source).Result;
        }
    }
}