using System;
using System.Collections.Generic;
using System.Linq;
using LinkUs.Modules.RemoteShell.Commands;
using LinkUs.Modules.RemoteShell.Events;

namespace LinkUs.Modules.RemoteShell
{
    public class ShellCommandHandler :
        IHandler<StartShell, ShellStarted>,
        IHandler<SendInputToShell>,
        IHandler<KillShell>
    {
        private readonly object _bus;
        private static readonly IDictionary<double, ShellProcessProxy> _activeRemoteShells = new Dictionary<double, ShellProcessProxy>();

        // ----- Constructor
        public ShellCommandHandler(object bus)
        {
            _bus = bus;
        }

        // ----- Public methods
        public ShellStarted Handle(StartShell command)
        {
            var remoteShell = new ShellProcessProxy(_bus);
            var processId = remoteShell.Start(command.CommandLine, command.Arguments.Cast<string>().ToArray());
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
        private static ShellProcessProxy GetRemoteShell(double processId)
        {
            ShellProcessProxy shellProcessProxy;
            if (!_activeRemoteShells.TryGetValue(processId, out shellProcessProxy)) {
                throw new Exception("Unable to find the remote shell");
            }
            return shellProcessProxy;
        }
    }
}