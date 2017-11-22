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
        private readonly FileDownloader _downloader;

        public DownloadFeature()
        {
            var downloadCommandHandler = new DownloadFileCommandHandler();
            var commandSender = new DirectCallCommandSender(downloadCommandHandler);
            _downloader = new FileDownloader(commandSender);
        }

        [Theory]
        [InlineData("Resources\\some_file.txt", "some_file_downloaded.txt")]
        [InlineData("Resources\\some_file.txt", "downloads\\some_file_downloaded.txt")]
        public async Task downloaded_file_is_identical_to_source_file(string sourceFilePath, string destinationFilePath)
        {
            // Acts
            await _downloader.DownloadAsync(sourceFilePath, destinationFilePath);

            // Asserts
            var sourceFile = new FileInfo(sourceFilePath);
            var downloadedFile = new FileInfo(destinationFilePath);
            Check.That(sourceFile.Length).IsEqualTo(downloadedFile.Length);
            Check.That(File.ReadAllText(sourceFilePath)).IsEqualTo(File.ReadAllText(destinationFilePath));
        }

        [Theory]
        [InlineData("Resources\\some_file.txt", "downloads\\")]
        public async Task when_destination_path_is_folder_downloaded_file_has_same_name_than_source_file(string sourceFilePath, string directoryPath)
        {
            // Acts
            await _downloader.DownloadAsync(sourceFilePath, directoryPath);

            // Asserts
            var destinationFilePath = Path.Combine(directoryPath, Path.GetFileName(sourceFilePath));
            Check.That(File.ReadAllText(sourceFilePath)).IsEqualTo(File.ReadAllText(destinationFilePath));
        }

    }
}