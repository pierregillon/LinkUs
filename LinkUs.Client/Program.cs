using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LinkUs.Core;

namespace LinkUs.Client
{
    class Program
    {
        private static readonly UTF8Encoding Encoding = new UTF8Encoding();
        private static readonly ManualResetEvent ManualResetEvent = new ManualResetEvent(false);
        private static readonly ISerializer Serializer = new JsonSerializer();
        private static readonly IDictionary<double, RemoteShell> _remoteShells = new Dictionary<double, RemoteShell>();

        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            while (true) {
                var connection = new SocketConnection();
                if (TryConnectSocketToHost(connection)) {
                    try {
                        var packageTransmitter = new PackageTransmitter(connection);
                        packageTransmitter.PackageReceived += (sender, package) => {
                            ProcessCommand(packageTransmitter, package);
                        };
                        packageTransmitter.Closed += (sender, eventArgs) => {
                            ManualResetEvent.Set();
                        };
                        ManualResetEvent.WaitOne();
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

        private static void ProcessCommand(PackageTransmitter transmitter, Package package)
        {
            Console.WriteLine(package);

            var command = Serializer.Deserialize<Command>(package.Content);
            if (command.Name == "ExecuteShellCommand") {
                var executeRemoteCommand = Serializer.Deserialize<ExecuteShellCommand>(package.Content);
                var remoteShell = new RemoteShell(transmitter, package, executeRemoteCommand);
                var processId = remoteShell.Start();
                remoteShell.ReadOutputAsync();
                _remoteShells.Add(processId, remoteShell);
            }
            else if (command.Name == "date") {
                var packageResponse = package.CreateResponsePackage(Serializer.Serialize(DateTime.Now.ToShortDateString()));
                transmitter.Send(packageResponse);
            }
            else if (command.Name == "ping") {
                var packageResponse = package.CreateResponsePackage(Serializer.Serialize("ok"));
                transmitter.Send(packageResponse);
            }
            else if (command.Name == typeof(SendInputToShellCommand).Name) {
                var sendInputToShellCommand = Serializer.Deserialize<SendInputToShellCommand>(package.Content);
                RemoteShell remoteShell;
                if (_remoteShells.TryGetValue(sendInputToShellCommand.ProcessId, out remoteShell)) {
                    remoteShell.Write(sendInputToShellCommand.Input);
                }
                else {
                    throw new Exception("Unable to find the remote shell");
                }
            }
            else if (command.Name == typeof(KillShellCommand).Name) {
                var killCommand = Serializer.Deserialize<KillShellCommand>(package.Content);
                RemoteShell remoteShell;
                if (_remoteShells.TryGetValue(killCommand.ProcessId, out remoteShell)) {
                    remoteShell.Kill();
                    _remoteShells.Remove(killCommand.ProcessId);
                }
                else {
                    throw new Exception("Unable to find the remote shell");
                }
            }
            else {
                var packageResponse = package.CreateResponsePackage(Serializer.Serialize("unknown command"));
                transmitter.Send(packageResponse);
            }
        }
    }
}