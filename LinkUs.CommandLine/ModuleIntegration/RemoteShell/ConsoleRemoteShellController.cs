using System;
using LinkUs.CommandLine.ConsoleLib;
using LinkUs.Core.Commands;
using LinkUs.Core.Packages;
using LinkUs.Modules.RemoteShell.Commands;
using LinkUs.Modules.RemoteShell.Events;

namespace LinkUs.CommandLine.ModuleIntegration.RemoteShell
{
    public class ConsoleRemoteShellController
    {
        private readonly IConsole _console;
        private readonly ICommandSender _commandSender;
        private bool _remoteShellIsActive;
        private CursorPosition _lastCursorPosition;

        // ----- Constructor
        public ConsoleRemoteShellController(IConsole console, ICommandSender commandSender)
        {
            _console = console;
            _commandSender = commandSender;
        }

        // ----- Public methods
        public void ProcessRemoteShellSession(ClientId target, string command)
        {
            command = command ?? AskUserForCommandToExecute();
            var processId = StartRemoteShell(target, command);
            var outputReceivedSubscription = _commandSender.Subscribe<ShellOutputReceived>(OnShellOutputReceived, response => response.ProcessId == processId);
            var shellEndedSubscription = _commandSender.Subscribe<ShellEnded>(OnShellEndedReceived, response => response.ProcessId == processId);

            try {
                ReadConsoleInputWhileShellActive(target, processId);
            }
            finally {
                outputReceivedSubscription.Dispose();
                shellEndedSubscription.Dispose();
            }
        }

        // ----- Callbacks
        private void OnShellOutputReceived(ShellOutputReceived @event)
        {
            if (string.IsNullOrEmpty(@event.Output) == false) {
                _console.Write(@event.Output);
                _lastCursorPosition = _console.GetCursorPosition();
            }
        }
        private void OnShellEndedReceived(ShellEnded @event)
        {
            _console.Write($"Process ended, exit code: {@event.ExitCode}. Press any key to continue.");
            _remoteShellIsActive = false;
        }

        // ----- Internal logic
        private string AskUserForCommandToExecute()
        {
            _console.Write("Command to execute on remote client > ");
            return _console.ReadLine();
        }
        private int StartRemoteShell(ClientId target, string commandInput)
        {
            var command = StartShell.Parse(commandInput);
            var response = _commandSender.ExecuteAsync<StartShell, ShellStarted>(command, target).Result;
            _console.WriteLine($"Shell started on remote host {target}, pid: {response.ProcessId}.");
            _remoteShellIsActive = true;
            _lastCursorPosition = _console.GetCursorPosition();
            return response.ProcessId;
        }
        private void ReadConsoleInputWhileShellActive(ClientId target, int processId)
        {
            var buffer = new char[1024];
            while (_remoteShellIsActive) {
                var bytesReadCount = _console.Read(buffer, 0, buffer.Length);
                if (_remoteShellIsActive && bytesReadCount > 0) {
                    ProcessInput(target, processId, new string(buffer, 0, bytesReadCount));
                }
            }
        }
        private void ProcessInput(ClientId target, int processId, string input)
        {
            if (input == "kill" + Environment.NewLine) {
                _commandSender.ExecuteAsync(new KillShell(processId), target);
            }
            else {
                _console.SetCursorPosition(_lastCursorPosition);
                _commandSender.ExecuteAsync(new SendInputToShell(input, processId), target);
            }
        }
    }
}