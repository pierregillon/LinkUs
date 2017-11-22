using System.Threading.Tasks;
using LinkUs.CommandLine.ModuleIntegration.Default;
using LinkUs.Core.Commands;

namespace LinkUs.Tests.Helpers {
    public class DirectCallCommandSender : IDedicatedCommandSender
    {
        private readonly object _receiver;

        public DirectCallCommandSender(object receiver)
        {
            _receiver = receiver;
        }

        public Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command)
        {
            var castedReceiver = (ICommandHandler<TCommand, TResponse>) _receiver;
            var result = castedReceiver.Handle(command);
            return Task.FromResult(result);
        }
    }
}