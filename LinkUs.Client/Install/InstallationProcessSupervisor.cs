using System;
using System.IO;

namespace LinkUs.Client.Install
{
    public class InstallationProcessSupervisor
    {
        private readonly IProcessManager _processManager;
        private readonly IEnvironment _environment;
        private readonly IInstaller _installer;

        // ----- Constructor
        public InstallationProcessSupervisor(
            IProcessManager processManager,
            IEnvironment environment,
            IInstaller installer)
        {
            _processManager = processManager;
            _environment = environment;
            _installer = installer;
        }

        // ----- Public methods
        public bool IsNewInstallationRequired()
        {
            return _installer.IsInstalled(_environment.ApplicationPath) == false;
        }
        public void AssureInstallationComplete()
        {
            try {
                _installer.CheckInstall(_environment.ApplicationPath);
            }
            catch (UnauthorizedAccessException) { }
        }
        public void SuperviseNewInstallation(string fileToInstall)
        {
            try {
                var installedPath = _installer.Install(fileToInstall);
                _processManager.StartProcessWithCurrentPrivileges(installedPath);
            }
            catch (HigherVersionAlreadyInstalled ex) {
                if (_processManager.IsProcessStarted(Path.GetFileName(ex.FilePath)) == false) {
                    var processStarted = _processManager.TryStartProcessWithElevatedPrivileges(fileToInstall);
                    if (processStarted == false) {
                        _processManager.StartProcess(ex.FilePath);
                    }
                }
            }
            catch (UnauthorizedAccessException) {
                var processStarted = _processManager.TryStartProcessWithElevatedPrivileges(fileToInstall);
                if (!processStarted) {
                    throw new InstallationFailed(new Exception("Cannot start process in administrator mode."));
                }
            }
            catch (Exception ex) {
                throw new InstallationFailed(ex);
            }
        }
    }
}