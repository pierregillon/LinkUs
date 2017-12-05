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
                _processManager.StartProcess(installedPath);
            }
            catch (HigherVersionAlreadyInstalled ex) {
                if (_processManager.IsProcessStarted(Path.GetFileName(ex.FilePath)) == false) {
                    _processManager.StartProcess(ex.FilePath);
                }
            }
            catch (Exception ex) {
                throw new InstallationFailed(ex);
            }
        }
    }
}