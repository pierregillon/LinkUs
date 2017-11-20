using System.Threading.Tasks;

namespace LinkUs.CommandLine
{
    public interface ICommandLineProcessor
    {
        Task Process(object commandLine);
    }
}