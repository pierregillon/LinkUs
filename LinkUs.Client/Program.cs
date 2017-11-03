﻿using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using LinkUs.Core.Shell.Commands;

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

            var command = Serializer.Deserialize<MessageDescriptor>(package.Content);
            if (command.Name == typeof(StartShell).Name) {
                var executeRemoteCommand = Serializer.Deserialize<StartShell>(package.Content);
                var remoteShell = new RemoteShell(transmitter, package, executeRemoteCommand);
                var processId = remoteShell.Start();
                remoteShell.ReadOutputAsync();
                _remoteShells.Add(processId, remoteShell);
            }
            else if (command.Name == typeof(Ping).Name) {
                var packageResponse = package.CreateResponsePackage(Serializer.Serialize("ok"));
                transmitter.Send(packageResponse);
            }
            else if (command.Name == typeof(SendInputToShell).Name) {
                var sendInputToShellCommand = Serializer.Deserialize<SendInputToShell>(package.Content);
                RemoteShell remoteShell;
                if (_remoteShells.TryGetValue(sendInputToShellCommand.ProcessId, out remoteShell)) {
                    remoteShell.Write(sendInputToShellCommand.Input);
                }
                else {
                    throw new Exception("Unable to find the remote shell");
                }
            }
            else if (command.Name == typeof(KillShell).Name) {
                var killCommand = Serializer.Deserialize<KillShell>(package.Content);
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