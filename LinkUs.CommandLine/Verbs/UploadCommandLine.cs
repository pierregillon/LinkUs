using CommandLine;

namespace LinkUs.CommandLine.Verbs
{
    public class UploadCommandLine : ClientTargettedCommandLine
    {
        [Option('s', "source", Required = true, HelpText = "The source file path to upload.")]
        public string SourceFilePath { get; set; }

        [Option('d', "destination", Required = true, HelpText = "The destination file path where the file wille be uploaded.")]
        public string DestinationFilePath { get; set; }
    }
}
