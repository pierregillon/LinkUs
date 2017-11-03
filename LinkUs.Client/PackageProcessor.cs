using System;
using System.Collections.Generic;
using System.Linq;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using LinkUs.Core.Shell.Commands;
using LinkUs.Core.Shell.Events;

namespace LinkUs.Client
{
    public class PackageProcessor
    {
        private readonly PackageTransmitter _transmitter;
        private readonly IDictionary<double, RemoteShell> _activeRemoteShells = new Dictionary<double, RemoteShell>();
        private readonly ISerializer _serializer = new JsonSerializer();

        // ----- Constructors
        public PackageProcessor(PackageTransmitter transmitter)
        {
            _transmitter = transmitter;
        }

        // ----- Public methods
        public void Process(Package package)
        {
            Console.WriteLine(package);

            var types = typeof(Message).Assembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Message)));
            var message = _serializer.Deserialize<MessageDescriptor>(package.Content);
            var messageType = types.Single(x => x.Name == message.Name);
            var messageInstance = (Message) _serializer.Deserialize(package.Content, messageType);

            if (messageInstance is StartShell) {
                var command = (StartShell) messageInstance;
                var remoteShell = new RemoteShell(_transmitter, package.Source, new JsonSerializer());
                var processId = remoteShell.Start(command);
                remoteShell.ReadOutputAsync();
                Answer(package, new ShellStarted {ProcessId = processId});
                _activeRemoteShells.Add(processId, remoteShell);
            }
            else if (messageInstance is Ping) {
                Answer(package, new PingOk("ok"));
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
                Answer(package, new ErrorMessage($"Unknown message : {messageInstance.Name}"));
            }
        }

        // ----- Utils
        private void Answer(Package package, Message response)
        {
            var bytes = _serializer.Serialize(response);
            var responsePackage = package.CreateResponsePackage(bytes);
            _transmitter.Send(responsePackage);
        }
    }
}