using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.CommandLine.ModuleIntegration.Default;
using LinkUs.CommandLine.ModuleIntegration.Default.FileTransferts;
using LinkUs.CommandLine.Verbs;

namespace LinkUs.CommandLine.Handlers
{
    public class ModuleCommandLineHandler :
        ICommandLineHandler<LoadModuleCommandLine>,
        ICommandLineHandler<UnloadModuleCommandLine>,
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

        public async Task Handle(LoadModuleCommandLine commandLine)
        {
            var remoteClient = await _server.FindRemoteClient(commandLine.Target);
            var moduleManager = new RemoteModuleManager(remoteClient);
            if (await moduleManager.IsModuleInstalled(commandLine.ModuleName)) {
                _console.WriteLine($"Module '{commandLine.ModuleName}' is already loaded. Did you mean 'unload-module' ?");
            }
            else {
                await moduleManager.LoadModule(commandLine.ModuleName);
            }
        }
        public async Task Handle(UnloadModuleCommandLine commandLine)
        {
            var remoteClient = await _server.FindRemoteClient(commandLine.Target);
            var moduleManager = new RemoteModuleManager(remoteClient);
            if (await moduleManager.IsModuleInstalled(commandLine.ModuleName) == false) {
                _console.WriteLine($"Module '{commandLine.ModuleName}' is not loaded. Did you mean 'load-module' ?");
            }
            else {
                await moduleManager.UnLoadModule(commandLine.ModuleName);
            }
        }
        public async Task Handle(ListModulesCommandLine commandLine)
        {
            if (commandLine.ListAvailableModules) {
                var modules = _moduleLocator.GetAvailableModules().ToArray();
                _console.WriteObjects(modules, "Name", "Version");
            }
            else {
                var remoteClient = await _server.FindRemoteClient(commandLine.Target);
                var moduleManager = new RemoteModuleManager(remoteClient);

                var allModules = _moduleLocator.GetAvailableModules().ToArray();
                var installedModules = await moduleManager.GetInstalledModules();
                foreach (var module in allModules) {
                    var installedModule = installedModules.SingleOrDefault(x => x.Name == module.Name);
                    if (installedModule != null) {
                        if (installedModule.IsLoaded) {
                            module.Status = ModuleInformation2.ModuleStatus.Installed;
                        }
                        else {
                            module.Status = ModuleInformation2.ModuleStatus.NotLoaded;
                        }
                    }
                    else {
                        module.Status = ModuleInformation2.ModuleStatus.Uninstalled;
                    }
                }
                _console.WriteObjects(allModules);
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
                var fullModulePath = _moduleLocator.GetFullPath(commandLine.ModuleName);
                await _console.WriteProgress("Downloading module", uploader, uploader.UploadAsync(fullModulePath, Path.GetFileName(fullModulePath)));
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
                _console.WriteLine("Unload module => [DONE]");
                await moduleManager.UnLoadModule(commandLine.ModuleName);
                //_console.WriteLine("Delete module => [DONE]");
                //var fullModulePath = _moduleLocator.GetFullPath(commandLine.ModuleName);
            }
        }
    }

    public class ModuleLocator
    {
        private const string MODULE_DIRECTORY = ".";

        public IEnumerable<ModuleInformation2> GetAvailableModules()
        {
            foreach (var filePath in Directory.GetFiles(MODULE_DIRECTORY, "LinkUs.Modules.*.dll")) {
                var assemblyName = AssemblyName.GetAssemblyName(filePath);
                yield return new ModuleInformation2 {
                    Name = assemblyName.Name,
                    Version = assemblyName.Version.ToString()
                };
            }
        }
        public string GetFullPath(string moduleName)
        {
            var fileName = moduleName + ".dll";
            return Path.Combine(MODULE_DIRECTORY, fileName);
        }
    }

    public class ModuleInformation2
    {
        public string Name { get; set; }
        public string Version { get; set; }
        public ModuleStatus Status { get; set; }

        public enum ModuleStatus
        {
            Unknown,
            Installed,
            NotLoaded,
            Uninstalled
        }
    }
}