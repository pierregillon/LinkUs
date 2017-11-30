using System;
using System.IO;
using System.Security;

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
        public void SuperviseNewInstallation()
        {
            try {
                var installedPath = _installer.Install(_environment.ApplicationPath);
                _processManager.StartProcessWithCurrentPrivileges(installedPath);
            }
            catch (HigherVersionAlreadyInstalled ex) {
                if (_processManager.IsProcessStarted(Path.GetFileName(ex.FilePath)) == false) {
                    var processStarted = _processManager.TryStartProcessWithElevatedPrivileges(ex.FilePath);
                    if (processStarted == false) {
                        _processManager.StartProcess(ex.FilePath);
                    }
                }
            }
            catch (UnauthorizedAccessException) {
                var processStarted = _processManager.TryStartProcessWithElevatedPrivileges(_environment.ApplicationPath);
                if (!processStarted) {
                    throw new InstallationFailed(new Exception("Cannot start process in administrator mode."));
                }
            }
            catch (SecurityException) {
                var processStarted = _processManager.TryStartProcessWithElevatedPrivileges(_environment.ApplicationPath);
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