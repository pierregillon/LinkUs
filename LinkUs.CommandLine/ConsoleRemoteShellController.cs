using System;
using System.Linq;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using LinkUs.Modules.RemoteShell.Commands;
using LinkUs.Modules.RemoteShell.Events;

namespace LinkUs.CommandLine
{
    public class ConsoleRemoteShellController
    {
        private readonly PackageTransmitter _packageTransmitter;
        private readonly CommandDispatcher _commandDispatcher;
        private readonly ClientId _target;
        private readonly ISerializer _serializer;
        private bool _remoteShellIsActive;
        private CursorPosition _lastCursorPosition;
        private int _processId;

        // ----- Constructor
        public ConsoleRemoteShellController(CommandDispatcher commandDispatcher, ClientId target, ISerializer serializer)
        {
            _packageTransmitter = commandDispatcher.PackageTransmitter;
            _commandDispatcher = commandDispatcher;
            _target = target;
            _serializer = serializer;
        }

        // ----- Public methods
        public void SendInputs()
        {
            _processId = StartRemoteShell();
            _remoteShellIsActive = true;
            _lastCursorPosition = _lastCursorPosition = new CursorPosition {
                Left = Console.CursorLeft,
                Top = Console.CursorTop
            };
            _packageTransmitter.PackageReceived += PackageTransmitterOnPackageReceived;

            try {
                ReadConsoleInputWhileShellActive();
            }
            finally {
                _packageTransmitter.PackageReceived -= PackageTransmitterOnPackageReceived;
            }
        }

        // ----- Callbacks
        private void PackageTransmitterOnPackageReceived(object sender, Package package)
        {
            var command = _serializer.Deserialize<MessageDescriptor>(package.Content);
            if (command.CommandName == typeof(ShellOutputReceived).Name) {
                var response = _serializer.Deserialize<ShellOutputReceived>(package.Content);
                if (response.ProcessId != _processId) return;
                Console.Write(response.Output);
                _lastCursorPosition = new CursorPosition {
                    Left = Console.CursorLeft,
                    Top = Console.CursorTop
                };
            }
            else if (command.CommandName == typeof(ShellEnded).Name) {
                var response = _serializer.Deserialize<ShellEnded>(package.Content);
                if (response.ProcessId != _processId) return;
                Console.Write($"Process ended, exit code: {response.ExitCode}. Press any key to continue.");
                _remoteShellIsActive = false;
            }
        }

        // ----- Internal logic
        private int StartRemoteShell()
        {
            Console.Write("Command to execute on remote client > ");
            var commandInput = Console.ReadLine();
            var command = BuildStartShellCommand(commandInput);
            var response = _commandDispatcher.ExecuteAsync<StartShell, ShellStarted>(command, _target).Result;
            Console.WriteLine($"Shell started on remote host {_target}, pid: {response.ProcessId}.");
            return response.ProcessId;
        }
        private void ReadConsoleInputWhileShellActive()
        {
            var buffer = new char[1024];
            while (_remoteShellIsActive) {
                var bytesReadCount = Console.In.Read(buffer, 0, buffer.Length);
                if (_remoteShellIsActive && bytesReadCount > 0) {
                    ProcessInput(new string(buffer, 0, bytesReadCount));
                }
            }
        }
        private void ProcessInput(string input)
        {
            if (input == "kill" + Environment.NewLine) {
                SendObject(new KillShell(_processId));
            }
            else {
                Console.SetCursorPosition(_lastCursorPosition.Left, _lastCursorPosition.Top);
                SendObject(new SendInputToShell(input, _processId));
            }
        }
        private static StartShell BuildStartShellCommand(string commandInput)
        {
            var arguments = commandInput.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var commandLine = arguments.First();
            return new StartShell {
                CommandLine = commandLine,
                Arguments = arguments.Skip(1).OfType<object>().ToList()
            };
        }
        private void SendObject(object command)
        {
            var package = new Package(ClientId.Unknown, _target, _serializer.Serialize(command));
            _packageTransmitter.Send(package);
        }
    }
}