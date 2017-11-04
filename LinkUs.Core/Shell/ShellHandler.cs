using System;
using System.Collections.Generic;
using LinkUs.Core.Shell.Commands;
using LinkUs.Core.Shell.Events;

namespace LinkUs.Core.Shell
{
    public class ShellHandler :
        IHandler<StartShell, ShellStarted>,
        IHandler<SendInputToShell>,
        IHandler<KillShell>
    {
        private readonly IMessageTransmitter _messageTransmitter;
        private static readonly IDictionary<double, RemoteShell> _activeRemoteShells = new Dictionary<double, RemoteShell>();

        public ShellHandler(IMessageTransmitter messageTransmitter)
        {
            _messageTransmitter = messageTransmitter;
        }

        public ShellStarted Handle(StartShell command)
        {
            var remoteShell = new RemoteShell(_messageTransmitter);
            var processId = remoteShell.Start(command);
            remoteShell.ReadOutputAsync();
            _activeRemoteShells.Add(processId, remoteShell);
            return new ShellStarted {ProcessId = processId};
        }
        public void Handle(SendInputToShell command)
        {
            var remoteShell = GetRemoteShell(command.ProcessId);
            remoteShell.Write(command.Input);
        }
        public void Handle(KillShell command)
        {
            var remoteShell = GetRemoteShell(command.ProcessId);
            remoteShell.Kill();
            _activeRemoteShells.Remove(command.ProcessId);
        }

        // ----- Internal logics
        private RemoteShell GetRemoteShell(double processId)
        {
            RemoteShell remoteShell;
            if (!_activeRemoteShells.TryGetValue(processId, out remoteShell)) {
                throw new Exception("Unable to find the remote shell");
            }
            return remoteShell;
        }
    }
}