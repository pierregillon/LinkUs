using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using LinkUs.Client.Infrastructure;
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
            var application = Ioc.GetInstance<Application>();
            if (args.Any(x => x == "--debug")) {
                AllocConsole();
                AttachConsole((uint) Process.GetCurrentProcess().Id);
                application.Run(debug: true);
                FreeConsole();
            }
            else {
                application.Run(debug: false);
            }
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