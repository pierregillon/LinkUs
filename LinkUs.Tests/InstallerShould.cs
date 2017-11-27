using System;
using LinkUs.Client;
using LinkUs.Client.Install;
using NFluent;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
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
        private readonly IProcessManager _processManager;

        public InstallerShould()
        {
            _environment = Substitute.For<IEnvironment>();
            _registry = Substitute.For<IRegistry>();
            _fileService = Substitute.For<IFileService>();
            _fileService.GetFileNameCopiedFromExisting(Arg.Any<string>()).Returns(RANDOM_APPLICATION_NAME);
            _processManager = Substitute.For<IProcessManager>();

            _installer = new Installer(_environment, _fileService, _registry, _processManager);
        }

        [Fact]
        public void do_nothing_when_application_already_installed()
        {
            // Arranges
            ConfigureEnvironmentApplicationInstalled(@"c:\windows\system32\app.exe");

            // Acts
            _installer.Install();

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
            _installer.Install();

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
            _installer.Install();

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
            _installer.Install();

            // Asserts
            _registry.Received(1).SetFileLocation($@"C:\WINDOWS\{plateformDirectory}\{RANDOM_APPLICATION_NAME}");
        }

        [Theory]
        [InlineData(false, @"system32")]
        [InlineData(true, @"SysWOW64")]
        public void install_application_plateform_directory(bool is64BitPlateform, string plateformDirectory)
        {
            // Arranges
            ConfigureCurrentApplicationIs(SOME_APPLICATION_PATH);
            ConfigureEnvironmentNoApplicationInstalled();
            _environment.Is64Bit.Returns(is64BitPlateform);

            // Acts
            _installer.Install();

            // Asserts
            _fileService.Received(1).Copy(SOME_APPLICATION_PATH, $@"C:\WINDOWS\{plateformDirectory}\{RANDOM_APPLICATION_NAME}");
        }

        [Fact]
        public void start_new_process_in_administrator_mode_when_unauthorized_access()
        {
            // Arrange
            ConfigureCurrentApplicationIs(SOME_APPLICATION_PATH);
            ConfigureEnvironmentNoApplicationInstalled();
            ConfigureProcessHasNoAdministratorPrivileges();

            // Acts
            _installer.Install();

            // Arranges
            _processManager.Received(1).StartProcessWithElevatedPrivileges(SOME_APPLICATION_PATH);
        }

        [Fact]
        public void abort_installation_if_new_process_with_elevated_privilege_was_started()
        {
            // Arrange
            ConfigureCurrentApplicationIs(SOME_APPLICATION_PATH);
            ConfigureEnvironmentNoApplicationInstalled();
            ConfigureProcessHasNoAdministratorPrivileges();

            _processManager
                .StartProcessWithElevatedPrivileges(SOME_APPLICATION_PATH)
                .Returns(true);

            // Acts
            var status = _installer.Install();

            // Arranges
            Check.That(status).IsEqualTo(InstallationStatus.Aborted);
        }

        [Fact]
        public void fail_installation_if_new_process_with_elevated_privilege_could_not_started()
        {
            // Arrange
            ConfigureEnvironmentNoApplicationInstalled();
            ConfigureProcessHasNoAdministratorPrivileges();

            _processManager
                .StartProcessWithElevatedPrivileges(SOME_APPLICATION_PATH)
                .Returns(false);

            // Acts
            var status = _installer.Install();

            // Arranges
            Check.That(status).IsEqualTo(InstallationStatus.Failed);
        }

        // ----- Utils
        private void ConfigureEnvironmentNoApplicationInstalled()
        {
            _registry.GetFileLocation().Returns((string) null);
            _registry.IsRegisteredAtStartup(SOME_APPLICATION_PATH).Returns(false);
        }
        private void ConfigureEnvironmentApplicationInstalled(string exePath)
        {
            _environment.ApplicationPath.Returns(exePath);
            _registry.GetFileLocation().Returns(exePath);
            _registry.IsRegisteredAtStartup(exePath).Returns(true);
        }
        private void ConfigureProcessHasNoAdministratorPrivileges()
        {
            _fileService
                .When(x => x.Copy(Arg.Any<string>(), Arg.Any<string>()))
                .Do(x => {
                    throw new UnauthorizedAccessException();
                });
        }
        private void ConfigureCurrentApplicationIs(string someApplicationPath)
        {
            _environment.ApplicationPath.Returns(someApplicationPath);
        }
    }
}