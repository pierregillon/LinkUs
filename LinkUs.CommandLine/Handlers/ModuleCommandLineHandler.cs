using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.ModuleIntegration;
using LinkUs.CommandLine.ModuleIntegration.Default;
using LinkUs.CommandLine.ModuleIntegration.Default.FileTransferts;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class ModuleCommandLineHandler :
        ICommandLineHandler<ListModulesCommandLine>,
        ICommandLineHandler<InstallModuleCommandLine>,
        ICommandLineHandler<UninstallModuleCommandLine>
    {
        private readonly IConsole _console;
        private readonly RemoteServer _server;
        private readonly ModuleLocator _moduleLocator;

        public ModuleCommandLineHandler(
            IConsole console,
            RemoteServer server,
            ModuleLocator moduleLocator)
        {
            _console = console;
            _server = server;
            _moduleLocator = moduleLocator;
        }

        public async Task Handle(ListModulesCommandLine commandLine)
        {
            if (string.IsNullOrEmpty(commandLine.Target)) {
                var modules = _moduleLocator.GetAvailableModules().ToArray();
                _console.WriteObjects(modules, "Name", "Description");
            }
            else {
                var remoteClient = await _server.FindRemoteClient(commandLine.Target);
                var moduleManager = new RemoteModuleManager(remoteClient);
                var availableModules = _moduleLocator.GetAvailableModules().ToArray();
                var installedModules = await moduleManager.GetInstalledModules();
                foreach (var module in availableModules) {
                    if (installedModules.Any(x => x.Name == module.Name)) {
                        _console.WriteWithColor("[enabled]", ConsoleColor.Green);
                    }
                    _console.WriteLine($"\t{module.Name}\t{module.Description}");
                }
            }
        }
        public async Task Handle(InstallModuleCommandLine commandLine)
        {
            var remoteClient = await _server.FindRemoteClient(commandLine.Target);
            var moduleManager = new RemoteModuleManager(remoteClient);
            if (await moduleManager.IsModuleInstalled(commandLine.ModuleName)) {
                _console.WriteLine($"Module '{commandLine.ModuleName}' is already installed. Did you mean 'uninstall-module' ?");
            }
            else {
                var uploader = new FileUploader(remoteClient);
                var module = _moduleLocator.GetAvailableModule(commandLine.ModuleName);
                var remoteModuleDirectoryPath = await moduleManager.GetModuleDirectory();
                var remoteFilePath = Path.Combine(remoteModuleDirectoryPath, Path.GetRandomFileName());
                await _console.WriteProgress("Uploading module", uploader, uploader.UploadAsync(module.FileLocation, remoteFilePath));
                await _console.WriteTaskStatus("Loading module    ", moduleManager.LoadModule(commandLine.ModuleName));
                _console.WriteLine($"Module '{commandLine.ModuleName}' was successfully installed.");
            }
        }
        public async Task Handle(UninstallModuleCommandLine commandLine)
        {
            var remoteClient = await _server.FindRemoteClient(commandLine.Target);
            var moduleManager = new RemoteModuleManager(remoteClient);
            if (await moduleManager.IsModuleInstalled(commandLine.ModuleName) == false) {
                _console.WriteLine($"Module '{commandLine.ModuleName}' is not installed. Did you mean 'install-module' ?");
            }
            else {
                var moduleInformation = await moduleManager.GetInstalledModule(commandLine.ModuleName);
                await _console.WriteTaskStatus("Unload module", moduleManager.UnLoadModule(moduleInformation.Name));
                await _console.WriteTaskStatus("Delete file  ", moduleManager.DeleteFile(moduleInformation.FileLocation));
                _console.WriteLine($"Module '{commandLine.ModuleName}' was successfully uninstalled.");
            }
        }
    }
}