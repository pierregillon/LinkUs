using System.Collections.Generic;
using System.Diagnostics;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Modules.Commands;
using LinkUs.Core.PingLib;

namespace LinkUs.CommandLine
{
    public class RemoteClient
    {
        private readonly CommandDispatcher _commandDispatcher;

        public RemoteClient(CommandDispatcher commandDispatcher)
        {
            _commandDispatcher = commandDispatcher;
        }

        public long Ping(ClientId target)
        {
            var stopWatch = new Stopwatch();
            stopWatch.Start();
            var pingCommand = new Ping();
            _commandDispatcher.ExecuteAsync<Ping, PingOk>(pingCommand, target).Wait();
            stopWatch.Stop();
            return stopWatch.ElapsedMilliseconds;
        }
        public IEnumerable<ModuleInformation> GetModules(ClientId targetId)
        {
            var command = new ListModules();
            var response = _commandDispatcher.ExecuteAsync<ListModules, ModuleInformationResponse>(command, targetId).Result;
            return response.ModuleInformations;
        }
        public bool LoadModule(ClientId target, string moduleName)
        {
            var command = new LoadModule(moduleName);
            var isSucceded = _commandDispatcher.ExecuteAsync<LoadModule, bool>(command, target).Result;
            return isSucceded;
        }
        public bool UnLoadModule(ClientId target, string moduleName)
        {
            var command = new UnloadModule(moduleName);
            var isSucceded = _commandDispatcher.ExecuteAsync<UnloadModule, bool>(command, target).Result;
            return isSucceded;
        }
    }
}