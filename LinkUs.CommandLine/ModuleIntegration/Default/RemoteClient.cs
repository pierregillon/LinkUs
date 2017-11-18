using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LinkUs.CommandLine.FileTransferts;
using LinkUs.Core.Commands;
using LinkUs.Core.Packages;
using LinkUs.Modules.Default.Modules;
using LinkUs.Modules.Default.Modules.Commands;
using LinkUs.Modules.Default.PingLib;
using LinkUs.Responses;

namespace LinkUs.CommandLine.ModuleIntegration.Default
{
    public class RemoteClient
    {
        private readonly ICommandSender _commandSender;

        public ClientId Id { get; }
        public ConnectedClient Information { get; }

        public RemoteClient(ICommandSender commandSender, ConnectedClient information)
        {
            Id = ClientId.Parse(information.Id);
            Information = information;
            _commandSender = commandSender;
        }

        public async Task<long> Ping()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var pingCommand = new Ping();
            await _commandSender.ExecuteAsync<Ping, PingOk>(pingCommand, Id);
            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
        public async Task<IReadOnlyCollection<ModuleInformation>> GetModules()
        {
            var command = new ListModules();
            var response = await _commandSender.ExecuteAsync<ListModules, ModuleInformation[]>(command, Id);
            return response;
        }
        public Task<bool> LoadModule(string moduleName)
        {
            var command = new LoadModule(moduleName);
            return _commandSender.ExecuteAsync<LoadModule, bool>(command, Id);
        }
        public Task<bool> UnLoadModule(string moduleName)
        {
            var command = new UnloadModule(moduleName);
            return _commandSender.ExecuteAsync<UnloadModule, bool>(command, Id);
        }
        public FileUploader GetFileUploader()
        {
            return new FileUploader(_commandSender, Id);
        }
        public FileDownloader GetFileDownloader()
        {
            return new FileDownloader(_commandSender, Id);
        }
    }
}