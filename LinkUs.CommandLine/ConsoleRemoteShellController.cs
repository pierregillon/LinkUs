using System;
using System.Linq;
using LinkUs.Core;
using LinkUs.Core.Commands;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using LinkUs.Core.Packages;
using LinkUs.Modules.RemoteShell.Commands;
using LinkUs.Modules.RemoteShell.Events;

namespace LinkUs.CommandLine
{
    public class ConsoleRemoteShellController
    {
        private readonly PackageTransmitter _packageTransmitter;
        private readonly ICommandSender _commandSender;
        private readonly ICommandSerializer _serializer;
        private bool _remoteShellIsActive;
        private CursorPosition _lastCursorPosition;
        private int _processId;

        // ----- Constructor
        public ConsoleRemoteShellController(ICommandSender commandSender, PackageTransmitter package, ICommandSerializer serializer)
        {
            _packageTransmitter = package;
            _commandSender = commandSender;
            _serializer = serializer;
        }

        // ----- Public methods
        public void SendInputs(ClientId target)
        {
            _processId = StartRemoteShell(target);
            _remoteShellIsActive = true;
            _lastCursorPosition = _lastCursorPosition = new CursorPosition {
                Left = Console.CursorLeft,
                Top = Console.CursorTop
            };
            _packageTransmitter.PackageReceived += PackageTransmitterOnPackageReceived;

            try {
                ReadConsoleInputWhileShellActive(target);
            }
            finally {
                _packageTransmitter.PackageReceived -= PackageTransmitterOnPackageReceived;
            }
        }

        // ----- Callbacks
        private void PackageTransmitterOnPackageReceived(object sender, Package package)
        {
            try {
                var command = _serializer.Deserialize<CommandDescriptor>(package.Content);
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
            catch (Exception ex) {
                Console.WriteLine(ex);
            }
        }

        // ----- Internal logic
        private int StartRemoteShell(ClientId target)
        {
            Console.Write("Command to execute on remote client > ");
            var commandInput = Console.ReadLine();
            var command = BuildStartShellCommand(commandInput);
            var response = _commandSender.ExecuteAsync<StartShell, ShellStarted>(command, target).Result;
            Console.WriteLine($"Shell started on remote host {target}, pid: {response.ProcessId}.");
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
                SendObject(target, new KillShell(_processId));
            }
            else {
                Console.SetCursorPosition(_lastCursorPosition.Left, _lastCursorPosition.Top);
                SendObject(target, new SendInputToShell(input, _processId));
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
        private void SendObject(ClientId target, object command)
        {
            var package = new Package(ClientId.Unknown, target, _serializer.Serialize(command));
            _packageTransmitter.Send(package);
        }
    }
}