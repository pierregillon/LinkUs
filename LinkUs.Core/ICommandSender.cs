using System.Threading.Tasks;
using LinkUs.Core.Connection;

namespace LinkUs.Core
{
    public interface ICommandSender
    {
        Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command, ClientId destination = null, ClientId source = null);
        void ExecuteAsync<TCommand>(TCommand command, ClientId destination = null);
    }
}