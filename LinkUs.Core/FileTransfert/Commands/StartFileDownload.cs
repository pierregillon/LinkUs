namespace LinkUs.Core.FileTransfert.Commands
{
    public class StartFileDownload : StartFileUpload
    {
        public string SourceFilePath { get; set; }
    }
}