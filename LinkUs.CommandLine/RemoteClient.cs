using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Modules.Commands;
using LinkUs.Core.PingLib;

namespace LinkUs.CommandLine
{
    public class RemoteClient
    {
        private readonly ICommandSender _commandSender;

        public RemoteClient(ICommandSender commandSender)
        {
            _commandSender = commandSender;
        }

        public async Task<long> Ping(ClientId target)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var pingCommand = new Ping();
            await _commandSender.ExecuteAsync<Ping, PingOk>(pingCommand, target);
            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
        public async Task<IReadOnlyCollection<ModuleInformation>> GetModules(ClientId targetId)
        {
            var command = new ListModules();
            var response = await _commandSender.ExecuteAsync<ListModules, ModuleInformation[]>(command, targetId);
            return response;
        }
        public Task<bool> LoadModule(ClientId target, string moduleName)
        {
            var command = new LoadModule(moduleName);
            return _commandSender.ExecuteAsync<LoadModule, bool>(command, target);
        }
        public Task<bool> UnLoadModule(ClientId target, string moduleName)
        {
            var command = new UnloadModule(moduleName);
            return _commandSender.ExecuteAsync<UnloadModule, bool>(command, target);
        }
        public FileUploader GetFileUploader(ClientId target)
        {
            return new FileUploader(_commandSender, target);
        }

    }
}