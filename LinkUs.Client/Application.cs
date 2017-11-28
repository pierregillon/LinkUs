using System;
using System.IO;
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
        private readonly IEnvironment _environment;
        private readonly IProcessManager _processManager;

        public Application(
            Ioc ioc,
            ModuleManager moduleManager,
            Installer installer,
            ServerBrowser serverBrowser,
            IEnvironment environment,
            IProcessManager processManager)
        {
            _ioc = ioc;
            _moduleManager = moduleManager;
            _installer = installer;
            _serverBrowser = serverBrowser;
            _environment = environment;
            _processManager = processManager;
        }

        // ----- Public methods
        public void Run(bool debug)
        {
            if (debug) {
                LoadModules();
                FindHostAndProcessRequests();
                return;
            }

            if (_installer.WellLocated()) {
                // Already moved to correct location, so no uac from here.
                _installer.CheckInstall();
                LoadModules();
                FindHostAndProcessRequests();
            }
            else {
                try {
                    // Uac allowed here
                    var installedPath = _installer.Install();
                    if (installedPath != null) {
                        _processManager.StartProcessWithCurrentPrivileges(installedPath);
                    }
                }
                catch (HigherVersionAlreadyInstalled ex) {
                    if (_processManager.IsProcessStarted(Path.GetFileName(ex.FilePath)) == false) {
                        var processStarted = _processManager.TryStartProcessWithElevatedPrivileges(_environment.ApplicationPath);
                        if (processStarted == false) {
                            _processManager.StartProcess(ex.FilePath);
                        }
                    }
                }
                catch (UnauthorizedAccessException) {
                    var processStarted = _processManager.TryStartProcessWithElevatedPrivileges(_environment.ApplicationPath);
                    if (!processStarted) {
                        LoadModules();
                        FindHostAndProcessRequests();
                    }
                }
            }
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