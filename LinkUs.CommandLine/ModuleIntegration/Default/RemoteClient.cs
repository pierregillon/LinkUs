using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LinkUs.Core.Commands;
using LinkUs.Core.Packages;
using LinkUs.Modules.Default.Modules;
using LinkUs.Modules.Default.Modules.Commands;
using LinkUs.Modules.Default.PingLib;
using LinkUs.Responses;

namespace LinkUs.CommandLine.ModuleIntegration.Default
{
    public class RemoteClient : IDedicatedCommandSender
    {
        private readonly ICommandSender _commandSender;

        public ClientId TargetId { get; }
        public ConnectedClient Information { get; }

        public RemoteClient(ICommandSender commandSender, ConnectedClient information)
        {
            TargetId = ClientId.Parse(information.Id);
            Information = information;
            _commandSender = commandSender;
        }

        public Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command)
        {
            return _commandSender.ExecuteAsync<TCommand, TResponse>(command, TargetId);
        }
        public async Task<long> Ping()
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var pingCommand = new Ping();
            await _commandSender.ExecuteAsync<Ping, PingOk>(pingCommand, TargetId);
            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
        public async Task<IReadOnlyCollection<ModuleInformation>> GetModules()
        {
            var command = new ListModules();
            var response = await _commandSender.ExecuteAsync<ListModules, ModuleInformation[]>(command, TargetId);
            return response;
        }
        public Task<bool> LoadModule(string moduleName)
        {
            var command = new LoadModule(moduleName);
            return _commandSender.ExecuteAsync<LoadModule, bool>(command, TargetId);
        }
        public Task<bool> UnLoadModule(string moduleName)
        {
            var command = new UnloadModule(moduleName);
            return _commandSender.ExecuteAsync<UnloadModule, bool>(command, TargetId);
        }
        public Task<bool> IsModuleInstalled(string moduleName)
        {
            var command = new IsModuleInstalled(moduleName);
            return _commandSender.ExecuteAsync<IsModuleInstalled, bool>(command, TargetId);
        }
    }
}