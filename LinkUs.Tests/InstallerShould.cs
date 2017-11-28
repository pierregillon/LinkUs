using System;
using LinkUs.Client;
using LinkUs.Client.Install;
using NFluent;
using NSubstitute;
using Xunit;

namespace LinkUs.Tests
{
    public class InstallerShould
    {
        private const string RANDOM_APPLICATION_NAME = "random.exe";
        private const string SOME_APPLICATION_PATH = @"c:\app.exe";

        private readonly Installer _installer;
        private readonly IEnvironment _environment;
        private readonly IRegistry _registry;
        private readonly IFileService _fileService;

        public InstallerShould()
        {
            _environment = Substitute.For<IEnvironment>();
            _registry = Substitute.For<IRegistry>();
            _fileService = Substitute.For<IFileService>();
            _fileService.GetFileNameCopiedFromExisting(Arg.Any<string>()).Returns(RANDOM_APPLICATION_NAME);

            _installer = new Installer(_environment, _fileService, _registry);
        }

        [Fact]
        public void do_nothing_when_application_already_installed()
        {
            // Arranges
            const string exeFilePath = @"c:\windows\system32\app.exe";
            ConfigureEnvironmentApplicationInstalled(exeFilePath);

            // Acts
            _installer.Install(exeFilePath);

            // Asserts
            _fileService.Received(0).Copy(Arg.Any<string>(), Arg.Any<string>());
            _registry.Received(0).AddFileToStartupRegistry(Arg.Any<string>());
            _registry.Received(0).RemoveFileFromStartupRegistry(Arg.Any<string>());
            _registry.Received(0).SetFileLocation(Arg.Any<string>());
        }

        [Fact]
        public void update_startup_registry_when_application_already_installed_but_startup_registry_invalid()
        {
            // Arrange
            ConfigureEnvironmentApplicationInstalled(SOME_APPLICATION_PATH);
            _registry.IsRegisteredAtStartup(SOME_APPLICATION_PATH).Returns(false);

            // Acts
            _installer.Install(SOME_APPLICATION_PATH);

            // Asserts
            _registry.Received(1).AddFileToStartupRegistry(SOME_APPLICATION_PATH);
        }

        [Theory]
        [InlineData(false, @"system32")]
        [InlineData(true, @"SysWOW64")]
        public void install_application_in_startup_registry(bool is64BitPlateform, string plateformDirectory)
        {
            // Arranges
            ConfigureEnvironmentNoApplicationInstalled();
            _environment.Is64Bit.Returns(is64BitPlateform);

            // Acts
            _installer.Install(SOME_APPLICATION_PATH);

            // Asserts
            _registry.Received(1).AddFileToStartupRegistry($@"C:\WINDOWS\{plateformDirectory}\{RANDOM_APPLICATION_NAME}");
        }

        [Theory]
        [InlineData(false, @"system32")]
        [InlineData(true, @"SysWOW64")]
        public void install_application_location_in_registry(bool is64BitPlateform, string plateformDirectory)
        {
            // Arranges
            ConfigureEnvironmentNoApplicationInstalled();
            _environment.Is64Bit.Returns(is64BitPlateform);

            // Acts
            _installer.Install(SOME_APPLICATION_PATH);

            // Asserts
            _registry.Received(1).SetFileLocation($@"C:\WINDOWS\{plateformDirectory}\{RANDOM_APPLICATION_NAME}");
        }

        [Theory]
        [InlineData(false, @"system32")]
        [InlineData(true, @"SysWOW64")]
        public void install_application_plateform_directory_and_return_it(bool is64BitPlateform, string plateformDirectory)
        {
            // Arranges
            ConfigureEnvironmentNoApplicationInstalled();
            _environment.Is64Bit.Returns(is64BitPlateform);

            // Acts
            var filePath = _installer.Install(SOME_APPLICATION_PATH);

            // Asserts
            var expectedFilePath = $@"C:\WINDOWS\{plateformDirectory}\{RANDOM_APPLICATION_NAME}";
            _fileService.Received(1).Copy(SOME_APPLICATION_PATH, expectedFilePath);
            Check.That(filePath).IsEqualTo(expectedFilePath);
        }

        [Theory]
        [InlineData(2, 1)]
        [InlineData(2, 2)]
        public void throw_error_when_installing_and_a_higher_version_is_already_installed(int installedVersion, int currentVersion)
        {
            // Arrange
            ConfigureVersionForExeFile("app1.exe", new Version(currentVersion, 0, 0, 0));
            ConfigureEnvironmentApplicationInstalled(@"C:\WINDOWS\system32\appv2.exe", new Version(installedVersion, 0, 0, 0));

            // Acts
            Action installation = () => _installer.Install("app1.exe");

            // Asserts
            Check.ThatCode(installation).Throws<HigherVersionAlreadyInstalled>();
        }

        [Theory]
        [InlineData(1, 2)]
        [InlineData(2, 9)]
        public void override_already_installed_version_if_lower(int installedVersion, int currentVersion)
        {
            // Arrange
            ConfigureVersionForExeFile("app1.exe", new Version(currentVersion, 0, 0, 0));
            ConfigureEnvironmentApplicationInstalled(@"C:\WINDOWS\system32\appv2.exe", new Version(installedVersion, 0, 0, 0));

            // Acts
            var filePath =_installer.Install("app1.exe");

            // Asserts
            Check.That(filePath).IsEqualTo($@"C:\WINDOWS\system32\random.exe");
        }

        // ----- Utils
        private void ConfigureEnvironmentNoApplicationInstalled()
        {
            _registry.GetFileLocation().Returns((string) null);
            _registry.IsRegisteredAtStartup(SOME_APPLICATION_PATH).Returns(false);
        }
        private void ConfigureEnvironmentApplicationInstalled(string exePath, Version version = null)
        {
            _registry.GetFileLocation().Returns(exePath);
            _registry.IsRegisteredAtStartup(exePath).Returns(true);
            _fileService.GetAssemblyVersion(exePath).Returns(version);
        }
        private void ConfigureVersionForExeFile(string exeFile, Version version)
        {
            _fileService.GetAssemblyVersion(exeFile).Returns(version);
        }
    }
}