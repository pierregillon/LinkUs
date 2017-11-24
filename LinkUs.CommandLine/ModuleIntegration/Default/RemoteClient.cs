using System.Diagnostics;
using System.Threading.Tasks;
using LinkUs.CommandLine.Handlers;
using LinkUs.Core.Commands;
using LinkUs.Core.Packages;
using LinkUs.Modules.Default.PingLib;
using LinkUs.Responses;

namespace LinkUs.CommandLine.ModuleIntegration.Default
{
    public class RemoteClient : IDedicatedCommandSender
    {
        private readonly ICommandSender _commandSender;

        // ----- Properties
        public ClientId TargetId { get; }
        public ConnectedClient Information { get; }

        // ----- Constructor
        public RemoteClient(ICommandSender commandSender, ConnectedClient information)
        {
            TargetId = ClientId.Parse(information.Id);
            Information = information;
            _commandSender = commandSender;
        }

        // ----- Public methods
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
    }
}