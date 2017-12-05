using System;
using System.IO;
using LinkUs.Client;
using LinkUs.Client.Install;
using NFluent;
using NSubstitute;
using Xunit;

namespace LinkUs.Tests
{
    public class InstallationProcessSupervisorShould
    {
        private readonly InstallationProcessSupervisor _installationProcessSupervisor;
        private readonly IProcessManager _processManager;
        private readonly IInstaller _installer;
        private readonly IEnvironment _environment;

        public InstallationProcessSupervisorShould()
        {
            _processManager = Substitute.For<IProcessManager>();
            _environment = Substitute.For<IEnvironment>();
            _environment.ApplicationPath.Returns(@"c:\users\toto\app.exe");
            _installer = Substitute.For<IInstaller>();
            _installationProcessSupervisor = new InstallationProcessSupervisor(_processManager, _environment, _installer);
        }

        [Fact]
        public void consider_new_installation_is_required_when_current_application_path_is_not_installed()
        {
            // Arranges
            _installer.IsInstalled(_environment.ApplicationPath).Returns(false);

            // Acts
            var required = _installationProcessSupervisor.IsNewInstallationRequired();

            // Asserts
            Check.That(required).IsTrue();
        }

        [Fact]
        public void assure_installation_completed_must_check_install_of_current_application_path()
        {
            // Acts
            _installationProcessSupervisor.AssureInstallationComplete();

            // Asserts
            _installer.Received(1).CheckInstall(_environment.ApplicationPath);
        }

        [Fact]
        public void assure_installation_completed_must_skip_unauthorized_exception_from_installer()
        {
            // Arranges
            UserHasNotEnoughRightToInstall();

            // Acts
            Action action = () => _installationProcessSupervisor.AssureInstallationComplete();

            // Asserts
            Check.ThatCode(action).Not.Throws<UnauthorizedAccessException>();
        }

        [Fact]
        public void install_current_application()
        {
            // Acts
            _installationProcessSupervisor.SuperviseNewInstallation();

            // Asserts
            _installer.Received(1).Install(_environment.ApplicationPath);
        }

        [Fact]
        public void do_nothing_if_a_higher_version_is_installed_and_runs()
        {
            // Arranges
            const string higherVersionExe = @"c:\higherversion.exe";
            AHigherVersionIsInstalledAndRuns(higherVersionExe);

            // Acts
            _installationProcessSupervisor.SuperviseNewInstallation();

            // Asserts
            _processManager.Received(0).StartProcess(higherVersionExe);
        }

        [Fact]
        public void start_installed_higher_version_if_not_running()
        {
            // Arranges
            const string higherVersionExe = @"c:\higherversion.exe";
            AHigherVersionIsInstalledButDoesNotRun(higherVersionExe);

            // Acts
            _installationProcessSupervisor.SuperviseNewInstallation();

            // Asserts
            _processManager.Received(1).StartProcess(higherVersionExe);
        }

        // ----- Utils
        private void AHigherVersionIsInstalledButDoesNotRun(string higherVersionExe)
        {
            _installer.When(x => x.Install(_environment.ApplicationPath))
                      .Do(x => {
                          throw new HigherVersionAlreadyInstalled(higherVersionExe);
                      });
            _processManager.IsProcessStarted(Path.GetFileName(higherVersionExe)).Returns(false);
        }
        private void AHigherVersionIsInstalledAndRuns(string higherVersionExe)
        {
            _installer.When(x => x.Install(_environment.ApplicationPath))
                      .Do(x => {
                          throw new HigherVersionAlreadyInstalled(higherVersionExe);
                      });
            _processManager.IsProcessStarted(Path.GetFileName(higherVersionExe)).Returns(true);
        }
        private void UserHasNotEnoughRightToInstall()
        {
            _installer
                .When(x => x.CheckInstall(_environment.ApplicationPath))
                .Do(x => {
                    throw new UnauthorizedAccessException();
                });

            _installer
                .When(x => x.Install(_environment.ApplicationPath))
                .Do(x => {
                    throw new UnauthorizedAccessException();
                });
        }
    }
}