using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using LinkUs.Core.Shell.Commands;
using LinkUs.Core.Shell.Events;

namespace LinkUs.Client
{
    class Program
    {
        private static readonly ManualResetEvent ManualResetEvent = new ManualResetEvent(false);
        private static readonly IDictionary<double, RemoteShell> _activeRemoteShells = new Dictionary<double, RemoteShell>();
        private static readonly ISerializer _serializer = new JsonSerializer();

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
                        packageTransmitter.Close();
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

            var types = typeof(Message).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Message)));
            var message = _serializer.Deserialize<MessageDescriptor>(package.Content);
            var messageType = types.Single(x => x.Name == message.Name);
            var messageInstance = (Message) _serializer.Deserialize(package.Content, messageType);

            if (messageInstance is StartShell) {
                var command = (StartShell) messageInstance;
                var remoteShell = new RemoteShell(transmitter, package.Source, new JsonSerializer());
                var processId = remoteShell.Start(command);
                remoteShell.ReadOutputAsync();
                var bytes = _serializer.Serialize(new ShellStarted {ProcessId = processId});
                var response = package.CreateResponsePackage(bytes);
                transmitter.Send(response);
                _activeRemoteShells.Add(processId, remoteShell);
            }
            else if (messageInstance is Ping) {
                var bytes = _serializer.Serialize(new PingOk("ok"));
                var response = package.CreateResponsePackage(bytes);
                transmitter.Send(response);
            }
            else if (messageInstance is SendInputToShell) {
                var sendInputToShellCommand = (SendInputToShell) messageInstance;
                RemoteShell remoteShell;
                if (_activeRemoteShells.TryGetValue(sendInputToShellCommand.ProcessId, out remoteShell)) {
                    remoteShell.Write(sendInputToShellCommand.Input);
                }
                else {
                    throw new Exception("Unable to find the remote shell");
                }
            }
            else if (messageInstance is KillShell) {
                var killCommand = (KillShell) messageInstance;
                RemoteShell remoteShell;
                if (_activeRemoteShells.TryGetValue(killCommand.ProcessId, out remoteShell)) {
                    remoteShell.Kill();
                    _activeRemoteShells.Remove(killCommand.ProcessId);
                }
                else {
                    throw new Exception("Unable to find the remote shell");
                }
            }
            else {
                var bytes = _serializer.Serialize(new ErrorMessage($"Unknown message : {messageInstance.Name}"));
                var response = package.CreateResponsePackage(bytes);
                transmitter.Send(response);
            }
        }
    }
}