using System.Threading.Tasks;

namespace LinkUs.CommandLine
{
    public interface ICommandLineDispatcher
    {
        Task Dispatch(object commandLine);
    }
}