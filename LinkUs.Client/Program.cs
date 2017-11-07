using System;
using System.Net.Sockets;
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
        private static readonly ManualResetEvent ManualResetEvent = new ManualResetEvent(false);

        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            var moduleManager = new ModuleManager();
            LoadModules(moduleManager);
            ConnectToHostAndProcessCommands(moduleManager);
        }

        // ----- Internal logics
        private static void LoadModules(ModuleManager moduleManager)
        {
            var packageParser = new PackageParser(new JsonSerializer());
            var moduleLocator = new ExternalAssemblyModuleLocator();
            moduleManager.Register(new LocalAssemblyModule(moduleManager, moduleLocator, packageParser));
            foreach (var module in new ExternalAssemblyModuleScanner(moduleLocator, packageParser).Scan()) {
                moduleManager.Register(module);
            }
        }
        private static void ConnectToHostAndProcessCommands(ModuleManager moduleManager)
        {
            while (true) {
                var connection = new SocketConnection();
                if (TryConnectSocketToHost(connection)) {
                    try {
                        ListenCommandsFromConnection(connection, moduleManager);
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex);
                    }
                }
                else {
                    Thread.Sleep(1000);
                }
            }
        }
        private static bool TryConnectSocketToHost(IConnection connection)
        {
            string host = "127.0.0.1";
            int port = 9000;

            try {
                Console.Write($"* Try to connect to host {host} on port {port} ... ");
                connection.Connect(host, port);
                Console.WriteLine("[SUCCESS]");
                return true;
            }
            catch (SocketException) {
                Console.WriteLine("[FAILED]");
                return false;
            }
        }
        private static void ListenCommandsFromConnection(IConnection connection, ModuleManager moduleManager)
        {
            var packageTransmitter = new PackageTransmitter(connection);
            var jsonSerializer = new JsonSerializer();
            var packageProcessor = new PackageProcessor(packageTransmitter, jsonSerializer, new PackageParser(new JsonSerializer()), moduleManager);
            packageTransmitter.PackageReceived += (sender, package) => {
                Console.WriteLine(package);
                packageProcessor.Process(package);
            };
            packageTransmitter.Closed += (sender, eventArgs) => {
                ManualResetEvent.Set();
            };
            var commandDispatcher = new CommandDispatcher(packageTransmitter, jsonSerializer);
            commandDispatcher.ExecuteAsync(new SetStatus {Status = "Provider"});
            ManualResetEvent.WaitOne();
            packageTransmitter.Close();
        }
    }
}