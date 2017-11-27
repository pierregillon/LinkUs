using System;
using System.Threading;
using LinkUs.Client.ClientInformation;
using LinkUs.Client.Install;
using LinkUs.Client.Modules;
using LinkUs.Core;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;

namespace LinkUs.Client
{
    public class Application
    {
        private readonly Ioc _ioc;
        private readonly ModuleManager _moduleManager;
        private readonly Installer _installer;
        private readonly ServerBrowser _serverBrowser;

        public Application(
            Ioc ioc,
            ModuleManager moduleManager,
            Installer installer,
            ServerBrowser serverBrowser)
        {
            _ioc = ioc;
            _moduleManager = moduleManager;
            _installer = installer;
            _serverBrowser = serverBrowser;
        }

        // ----- Public methods
        public void Run(bool debug)
        {
            if (!debug) {
                var status = _installer.Install();
                if (status == InstallationStatus.Aborted) {
                    return;
                }
            }

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
                var connection = FindAvailableHost();
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
        private IConnection FindAvailableHost()
        {
            var connection = _serverBrowser.SearchAvailableHost();
            _ioc.RegisterSingle(connection);
            return connection;
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