using System;
using System.Threading;
using LinkUs.Core;
using LinkUs.Core.ClientInformation;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using LinkUs.Core.Modules;

namespace LinkUs.Client
{
    class Program
    {
        static Program()
        {
            Ioc.Instance.RegisterSingle<ModuleManager>();
            Ioc.Instance.Register<RequestProcessor>();
            Ioc.Instance.Register<PackageParser>();
            Ioc.Instance.Register<PackageTransmitter>();
            Ioc.Instance.Register<PackageProcessor>();
            Ioc.Instance.Register<ISerializer, JsonSerializer>();
            Ioc.Instance.Register<ExternalAssemblyModuleLocator>();
            Ioc.Instance.Register<ExternalAssemblyModuleScanner>();
            Ioc.Instance.Register<ServerBrowser>();
            Ioc.Instance.Register<ICommandSender, CommandSender>();
        }

        static void Main(string[] args)
        {
            LoadModules();
            FindHostAndProcessRequests();
        }

        // ----- Internal logics
        private static void LoadModules()
        {
            var moduleManager = Ioc.Instance.GetInstance<ModuleManager>();
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
                    Ioc.Instance.UnregisterSingle<IConnection>();
                }
                Thread.Sleep(1000);
            }
        }
        private static IConnection FindAvailableHost()
        {
            var serverBrowser = Ioc.Instance.GetInstance<ServerBrowser>();
            var connection = serverBrowser.SearchAvailableHost();
            Ioc.Instance.RegisterSingle(connection);
            return connection;
        }
        private static void ProcessRequests()
        {
            var commandSender = Ioc.Instance.GetInstance<ICommandSender>();
            commandSender.ExecuteAsync(new SetStatus {Status = "Provider"});

            var requestProcessor = Ioc.Instance.GetInstance<RequestProcessor>();
            requestProcessor.ProcessRequests();
        }
    }
}