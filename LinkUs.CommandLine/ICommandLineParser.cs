namespace LinkUs.CommandLine
{
    public interface ICommandLineParser
    {
        object Parse(string[] arguments);
    }
}