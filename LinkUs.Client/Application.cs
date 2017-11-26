using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
        private readonly IEnvironment _environment;
        private readonly Installer _installer;
        private readonly ServerBrowser _serverBrowser;

        public Application(
            Ioc ioc,
            ModuleManager moduleManager,
            IEnvironment environment,
            Installer installer,
            ServerBrowser serverBrowser)
        {
            _ioc = ioc;
            _moduleManager = moduleManager;
            _environment = environment;
            _installer = installer;
            _serverBrowser = serverBrowser;
        }

        // ----- Public methods
        public void Run(bool debug)
        {
            if (!debug) {
                InitializeClient();
            }

            LoadModules();
            FindHostAndProcessRequests();
        }

        // ----- Internal logics
        private void InitializeClient()
        {
            try {
                var currentInstalledApplicationPath = _installer.GetCurrentInstalledApplicationPath();
                if (string.IsNullOrEmpty(currentInstalledApplicationPath)) {
                    _installer.Install();
                }
                else if (string.Equals(currentInstalledApplicationPath, _environment.ApplicationPath, StringComparison.InvariantCultureIgnoreCase)) {
                    _installer.CheckInstall();
                }
                else {
                    var installedAssemblyInfo = AssemblyName.GetAssemblyName(currentInstalledApplicationPath);
                    if (installedAssemblyInfo.Version >= Assembly.GetEntryAssembly().GetName().Version) {
                        var fileName = Path.GetFileName(currentInstalledApplicationPath);
                        var processes = Process.GetProcessesByName(fileName);
                        if (processes.Length == 0) {
                            Process.Start(new ProcessStartInfo(currentInstalledApplicationPath));
                            // todo: missing case where the existing client is not well installed.
                            // when restarted, it will try to checkinstall() and open uac! bad.
                        }
                        Environment.Exit(0);
                    }
                    else {
                        _installer.Install();
                    }
                }
            }
            catch (UnauthorizedAccessException) {
                if (TryRestartApplicationWithAdministratorPrivileges()) {
                    // We succeded to start a new process in admin mode
                    // we can quit.
                    Environment.Exit(0);
                }
                else {
                    // We failed to start the process in admin mode, we start
                    // the client
                }
            }
        }
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

        // ----- Utils
        private bool TryRestartApplicationWithAdministratorPrivileges()
        {
            try {
                var exeName = _environment.ApplicationPath;
                var startInfo = new ProcessStartInfo(exeName) {
                    Verb = "runas"
                };
                Process.Start(startInfo);
                return true;
            }
            catch (Win32Exception) {
                return false;
            }
        }
    }
}