using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class UploadFileCommandLine : ClientTargettedCommandLine
    {
        [Option('s', "source", Required = true, HelpText = "The local source file path to upload to the client.")]
        public string LocalSourceFilePath { get; set; }

        [Option('d', "destination", Required = true, HelpText = "The remote file path where the file will be uploaded.")]
        public string RemoteDestinationFilePath { get; set; }
    }
}
