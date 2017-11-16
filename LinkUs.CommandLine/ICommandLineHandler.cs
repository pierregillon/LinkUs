using System.Threading.Tasks;

namespace LinkUs.CommandLine
{
    public interface ICommandLineHandler<in TCommandLine>
    {
        Task Handle(TCommandLine commandLine);
    }
}