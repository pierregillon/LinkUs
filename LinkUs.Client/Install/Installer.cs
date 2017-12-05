using System;
using System.IO;

namespace LinkUs.Client.Install
{
    public class Installer : IInstaller
    {
        private readonly IFileService _fileService;
        private readonly IRegistry _registry;

        // ----- Constructor
        public Installer(
            IFileService fileService,
            IRegistry registry)
        {
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
            if (string.Equals(exeFile, _registry.GetFileLocation(), StringComparison.InvariantCultureIgnoreCase)) {
                _registry.ClearFileLocation();
            }
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
            var randomFileName = _fileService.GetRandomFileName(".exe");
            var targetFilePath = Path.Combine(GetInstallationDirectory(), randomFileName);

            _fileService.Copy(exeFile, targetFilePath);
            //_fileService.Hide(targetFilePath);
            _registry.AddFileToStartupRegistry(targetFilePath);
            _registry.SetFileLocation(targetFilePath);

            return targetFilePath;
        }
        private static string GetInstallationDirectory()
        {
            return Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        }
    }
}