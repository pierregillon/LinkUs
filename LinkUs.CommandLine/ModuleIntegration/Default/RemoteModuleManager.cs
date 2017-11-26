using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkUs.Client.FileManagement;
using LinkUs.Client.Modules;
using LinkUs.Client.Modules.Commands;

namespace LinkUs.CommandLine.ModuleIntegration.Default
{
    public class RemoteModuleManager
    {
        private readonly IDedicatedCommandSender _client;

        public RemoteModuleManager(IDedicatedCommandSender client)
        {
            _client = client;
        }

        // ----- Public methods
        public async Task<ModuleInformation> GetInstalledModule(string moduleName)
        {
            var modules = await GetInstalledModules();
            return modules.Single(x => x.Name == moduleName);
        }
        public async Task<IReadOnlyCollection<ModuleInformation>> GetInstalledModules()
        {
            var command = new ListModules();
            var response = await _client.ExecuteAsync<ListModules, ModuleInformation[]>(command);
            return response;
        }
        public Task LoadModule(string moduleName)
        {
            var command = new LoadModule(moduleName);
            return _client.ExecuteAsync<LoadModule, bool>(command);
        }
        public Task UnLoadModule(string moduleName)
        {
            var command = new UnloadModule(moduleName);
            return _client.ExecuteAsync<UnloadModule, bool>(command);
        }
        public Task<bool> IsModuleInstalled(string moduleName)
        {
            var command = new IsModuleInstalled(moduleName);
            return _client.ExecuteAsync<IsModuleInstalled, bool>(command);
        }
        public Task DeleteFile(string moduleName)
        {
            return _client.ExecuteAsync<DeleteFileCommand, bool>(new DeleteFileCommand { FilePath = moduleName });
        }
        public Task<string> GetModuleDirectory()
        {
            return Task.FromResult("ef3cA");
        }
    }
}