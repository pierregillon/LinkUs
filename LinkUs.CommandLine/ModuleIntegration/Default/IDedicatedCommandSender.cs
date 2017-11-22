using System.Threading.Tasks;
using LinkUs.Core.Packages;

namespace LinkUs.CommandLine.ModuleIntegration.Default
{
    public interface IDedicatedCommandSender
    {
        ClientId TargetId { get; }

        Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command);
    }
}