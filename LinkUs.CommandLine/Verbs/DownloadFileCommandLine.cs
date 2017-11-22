using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class DownloadFileCommandLine : ClientTargettedCommandLine
    {
        [Option('s', "source", 
            Required = true, 
            HelpText = "The remote source file path to download.")]
        public string RemoteSourceFilePath { get; set; }

        [Option('d', "destination", 
            Required = false, 
            HelpText = "The local path where the file should be downloaded. " +
                       "It can be a file or a directory. When it is a directory, the downloaded file name will be the " +
                       "remote file name. " +
                       "When not defined, the file will be downloaded at [lkus directory]/downloads/{cliendId}/{remoteFileName}")]
        public string LocalDestinationFilePath { get; set; }
    }
}