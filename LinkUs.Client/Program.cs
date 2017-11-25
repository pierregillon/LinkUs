using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using LinkUs.Core;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Core.Packages;
using LinkUs.Modules.Default.ClientInformation;
using LinkUs.Modules.Default.Modules;

namespace LinkUs.Client
{
    class Program
    {
        private static readonly Ioc Ioc = BuildIoc();

        static void Main(string[] args)
        {
            if (args.Any(x => x == "--debug")) {
                LoadModules();
                FindHostAndProcessRequests();
                return;
            }

            if (IsAdministrator() == false) {
                RestartApplicationWithAdministratorPrivileges();
                return;
            }

            var installer = Ioc.GetInstance<Installer>();
            if (IsWellLocated()) {
                installer.CheckInstall();
                LoadModules();
                FindHostAndProcessRequests();
            }
            else {
                var filePath = installer.Install();
                if (string.IsNullOrEmpty(filePath) == false) {
                    StartExecutableWithAdministratorPrivileges(filePath);
                }
            }
        }

        // ----- Internal logics
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
        private static bool IsAdministrator()
        {
            using (var identity = WindowsIdentity.GetCurrent()) {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }
        private static void RestartApplicationWithAdministratorPrivileges()
        {
            var exeName = Process.GetCurrentProcess().MainModule.FileName;
            StartExecutableWithAdministratorPrivileges(exeName);
        }
        private static void StartExecutableWithAdministratorPrivileges(string filePath)
        {
            var startInfo = new ProcessStartInfo(filePath) {
                Verb = "runas"
            };
            Process.Start(startInfo);
        }
        private static bool IsWellLocated()
        {
            var env = Ioc.GetInstance<IEnvironment>();
            var parentDirectory = Path.GetDirectoryName(env.ApplicationPath);
            return string.Equals(env.InstallationDirectory, parentDirectory, StringComparison.CurrentCultureIgnoreCase);
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
    }
}