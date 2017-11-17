using System;
using System.Threading;
using LinkUs.Core;
using LinkUs.Core.ClientInformation;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using LinkUs.Core.Modules;
using LinkUs.Core.Packages;

namespace LinkUs.Client
{
    class Program
    {
        private static readonly Ioc Ioc = BuildIoc();

        static void Main(string[] args)
        {
            LoadModules();
            FindHostAndProcessRequests();
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
            ioc.Register<ServerBrowser>();
            ioc.Register<ICommandSender, CommandSender>();
            ioc.Register<AssemblyHandlerScanner>();
            return ioc;
        }
    }
}