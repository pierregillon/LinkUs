using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class DownloadFileCommandLine : ClientTargettedCommandLine
    {
        [Option('s', "source", Required = true, HelpText = "The remote source file path to download.")]
        public string RemoteSourceFilePath { get; set; }

        [Option('d', "destination", Required = true, HelpText = "The local file path where the file wille be downloaded.")]
        public string LocalDestinationFilePath { get; set; }
    }
}