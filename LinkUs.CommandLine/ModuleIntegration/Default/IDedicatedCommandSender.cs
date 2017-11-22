using System.Threading.Tasks;

namespace LinkUs.CommandLine.ModuleIntegration.Default
{
    public interface IDedicatedCommandSender
    {
        Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command);
    }
}