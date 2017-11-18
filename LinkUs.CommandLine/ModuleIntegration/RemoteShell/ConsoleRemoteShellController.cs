using System;
using System.Linq;
using LinkUs.Core.Commands;
using LinkUs.Core.Packages;
using LinkUs.Modules.RemoteShell.Commands;
using LinkUs.Modules.RemoteShell.Events;

namespace LinkUs.CommandLine.ModuleIntegration.RemoteShell
{
    public class ConsoleRemoteShellController
    {
        private readonly ICommandSender _commandSender;
        private bool _remoteShellIsActive;
        private CursorPosition _lastCursorPosition;
        private int _processId;

        // ----- Constructor
        public ConsoleRemoteShellController(ICommandSender commandSender)
        {
            _commandSender = commandSender;
        }

        // ----- Public methods
        public void StartRemoteShellSession(ClientId target, string command)
        {
            command = command ?? AskUserForCommandToExecute();
            _processId = StartRemoteShell(target, command);
            _lastCursorPosition = _lastCursorPosition = new CursorPosition {
                Left = Console.CursorLeft,
                Top = Console.CursorTop
            };

            var outputReceivedSubscription = _commandSender.Subscribe<ShellOutputReceived>(OnShellOutputReceived, response => response.ProcessId == _processId);
            var shellEndedSubscription = _commandSender.Subscribe<ShellEnded>(OnShellEndedReceived, response => response.ProcessId == _processId);

            try {
                ReadConsoleInputWhileShellActive(target);
            }
            finally {
                outputReceivedSubscription.Dispose();
                shellEndedSubscription.Dispose();
            }
        }

        // ----- Callbacks
        private void OnShellOutputReceived(ShellOutputReceived @event)
        {
            Console.Write(@event.Output);
            _lastCursorPosition = new CursorPosition {
                Left = Console.CursorLeft,
                Top = Console.CursorTop
            };
        }
        private void OnShellEndedReceived(ShellEnded @event)
        {
            Console.Write($"Process ended, exit code: {@event.ExitCode}. Press any key to continue.");
            _remoteShellIsActive = false;
        }

        // ----- Internal logic
        private string AskUserForCommandToExecute()
        {
            Console.Write("Command to execute on remote client > ");
            return Console.ReadLine();
        }
        private int StartRemoteShell(ClientId target, string commandInput)
        {
            var command = BuildStartShellCommand(commandInput);
            var response = _commandSender.ExecuteAsync<StartShell, ShellStarted>(command, target).Result;
            Console.WriteLine($"Shell started on remote host {target}, pid: {response.ProcessId}.");
            _remoteShellIsActive = true;
            return response.ProcessId;
        }
        private void ReadConsoleInputWhileShellActive(ClientId target)
        {
            var buffer = new char[1024];
            while (_remoteShellIsActive) {
                var bytesReadCount = Console.In.Read(buffer, 0, buffer.Length);
                if (_remoteShellIsActive && bytesReadCount > 0) {
                    ProcessInput(target, new string(buffer, 0, bytesReadCount));
                }
            }
        }
        private void ProcessInput(ClientId target, string input)
        {
            if (input == "kill" + Environment.NewLine) {
                _commandSender.ExecuteAsync(new KillShell(_processId), target);
            }
            else {
                Console.SetCursorPosition(_lastCursorPosition.Left, _lastCursorPosition.Top);
                _commandSender.ExecuteAsync(new SendInputToShell(input, _processId), target);
            }
        }
        private static StartShell BuildStartShellCommand(string commandInput)
        {
            var arguments = commandInput.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            var commandLine = arguments.First();
            return new StartShell {
                CommandLine = commandLine,
                Arguments = arguments.Skip(1).OfType<object>().ToList()
            };
        }
    }
}