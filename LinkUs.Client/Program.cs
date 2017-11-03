using System;
using System.Collections.Generic;
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

        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            while (true) {
                var connection = new SocketConnection();
                if (TryConnectSocketToHost(connection)) {
                    try {
                        var messageTransmitter = new MessageTransmitter(new PackageTransmitter(connection), new JsonSerializer());
                        messageTransmitter.MessageReceived += envelop => {
                            ProcessCommand(messageTransmitter, envelop);
                        };
                        messageTransmitter.Closed += () => {
                            ManualResetEvent.Set();
                        };
                        ManualResetEvent.WaitOne();
                        messageTransmitter.Close();
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

        private static void ProcessCommand(MessageTransmitter transmitter, Envelop envelop)
        {
            Console.WriteLine(envelop.Message.Name);

            if (envelop.Message is StartShell) {
                var command = (StartShell) envelop.Message;
                var remoteShell = new RemoteShell(transmitter, envelop.Sender);
                var processId = remoteShell.Start(command);
                remoteShell.ReadOutputAsync();
                var returnEnvelop = envelop.CreateReturn(new ShellStarted {ProcessId = processId});
                transmitter.Send(returnEnvelop);
                _activeRemoteShells.Add(processId, remoteShell);
            }
            else if (envelop.Message is Ping) {
                var returnEnvelop = envelop.CreateReturn(new PingOk("ok"));
                transmitter.Send(returnEnvelop);
            }
            else if (envelop.Message is SendInputToShell) {
                var sendInputToShellCommand = (SendInputToShell) envelop.Message;
                RemoteShell remoteShell;
                if (_activeRemoteShells.TryGetValue(sendInputToShellCommand.ProcessId, out remoteShell)) {
                    remoteShell.Write(sendInputToShellCommand.Input);
                }
                else {
                    throw new Exception("Unable to find the remote shell");
                }
            }
            else if (envelop.Message is KillShell) {
                var killCommand = (KillShell) envelop.Message;
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
                var returnEnvelop = envelop.CreateReturn(new ErrorMessage($"Unknown message : {envelop.Message.Name}"));
                transmitter.Send(returnEnvelop);
            }
        }
    }
}