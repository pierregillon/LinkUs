using System.IO;
using System.Threading.Tasks;
using LinkUs.CommandLine.FileTransferts;
using LinkUs.Modules.Default.FileTransfert;
using LinkUs.Tests.Helpers;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class DownloadFeature
    {
        private string SOME_FILE = "Resources\\some_file.txt";
        private readonly FileDownloader _downloader;

        public DownloadFeature()
        {
            var downloadCommandHandler = new DownloadFileCommandHandler();
            var commandSender = new DirectCallCommandSender(downloadCommandHandler);
            _downloader = new FileDownloader(commandSender);
        }

        [Theory]
        [InlineData("some_file_downloaded.txt")]
        [InlineData("downloads\\some_file_downloaded.txt")]
        public async Task download_simple_file(string destinationFilePath)
        {
            // Acts
            await _downloader.DownloadAsync(SOME_FILE, destinationFilePath);

            // Asserts
            var sourceFile = new FileInfo(SOME_FILE);
            var downloadedFile = new FileInfo(destinationFilePath);
            Check.That(sourceFile.Length).IsEqualTo(downloadedFile.Length);
        }
    }
}