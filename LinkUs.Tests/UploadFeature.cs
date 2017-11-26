using System.IO;
using System.Threading.Tasks;
using LinkUs.Client.FileTransfert;
using LinkUs.CommandLine.ModuleIntegration.Default.FileTransferts;
using LinkUs.Tests.Helpers;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class UploadFeature
    {
        private string SOME_FILE = "Resources\\some_file2.txt";
        private readonly FileUploader _uploader;

        public UploadFeature()
        {
            var handler = new UploadFileCommandHandler();
            var commandSender = new DirectCallCommandSender(handler);
            _uploader = new FileUploader(commandSender);
        }

        [Theory]
        [InlineData("some_file_uploaded.txt")]
        [InlineData("uploads\\some_file_uploaded.txt")]
        public async Task download_simple_file(string destinationFilePath)
        {
            // Acts
            await _uploader.UploadAsync(SOME_FILE, destinationFilePath);

            // Asserts
            var sourceFile = new FileInfo(SOME_FILE);
            var downloadedFile = new FileInfo(destinationFilePath);
            Check.That(sourceFile.Length).IsEqualTo(downloadedFile.Length);
        }
    }
}