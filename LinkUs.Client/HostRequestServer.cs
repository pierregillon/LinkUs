using System;
using System.Threading;
using LinkUs.Client.ClientInformation;
using LinkUs.Client.Modules;
using LinkUs.Core;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;

namespace LinkUs.Client
{
    public class HostRequestServer
    {
        private readonly Ioc _ioc;
        private readonly ModuleManager _moduleManager;
        private readonly ServerBrowser _serverBrowser;

        // ----- Constructor
        public HostRequestServer(
            Ioc ioc,
            ModuleManager moduleManager,
            ServerBrowser serverBrowser)
        {
            _ioc = ioc;
            _moduleManager = moduleManager;
            _serverBrowser = serverBrowser;
        }

        // ----- Public methods
        public void Serve()
        {
            LoadModules();
            FindHostAndProcessRequests();
        }

        // ----- Internal logics
        private void LoadModules()
        {
            _moduleManager.LoadModules();
        }
        private void FindHostAndProcessRequests()
        {
            while (true) {
                var connection = _serverBrowser.SearchAvailableHost();
                _ioc.RegisterSingle(connection);
                try {
                    ProcessRequests();
                }
                catch (Exception ex) {
                    Console.WriteLine(ex);
                }
                finally {
                    connection.Close();
                    _ioc.UnregisterSingle<IConnection>();
                }
                Thread.Sleep(1000);
            }
        }
        private void ProcessRequests()
        {
            var commandSender = _ioc.GetInstance<ICommandSender>();
            commandSender.ExecuteAsync(new SetStatus { Status = "Provider" });

            var requestProcessor = _ioc.GetInstance<RequestProcessor>();
            requestProcessor.ProcessRequests();
        }
    }
}