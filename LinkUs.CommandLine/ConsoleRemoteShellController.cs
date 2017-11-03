using System;
using System.Linq;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using LinkUs.Core.Shell;

namespace LinkUs.CommandLine
{
    public class ConsoleRemoteShellController
    {
        private readonly PackageTransmitter _packageTransmitter;
        private readonly CommandDispatcher _commandDispatcher;
        private readonly ClientId _target;
        private readonly ISerializer _serializer;
        private bool _end;
        private CursorPosition _lastCursorPosition = new CursorPosition();
        private double _processId;

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
            Console.Write($"shell:{_target}> ");
            var commandInput = Console.ReadLine();
            var arguments = commandInput.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);
            var commandLine = arguments.First();
            var command = new ExecuteShellCommand {
                Name = "ExecuteShellCommand",
                CommandLine = commandLine,
                Arguments = arguments.Skip(1).OfType<object>().ToList()
            };
            var response = _commandDispatcher.ExecuteAsync<ExecuteShellCommand, ShellStartedResponse>(command, _target).Result;
            _processId = response.ProcessId;
            Console.WriteLine($"Shell started on remote host {_target}, pid: {response.ProcessId}.");

            _packageTransmitter.PackageReceived += PackageTransmitterOnPackageReceived;
            try {
                var buffer = new char[1024];
                while (_end == false) {
                    var bytesReadCount = Console.In.Read(buffer, 0, buffer.Length);
                    if (_end) {
                        break;
                    }
                    if (bytesReadCount > 0) {
                        var input = new string(buffer, 0, bytesReadCount);
                        if (input == "stop" + Environment.NewLine) {
                            SendObject(new KillShellCommand(_processId));
                        }
                        else {
                            Console.SetCursorPosition(_lastCursorPosition.Left, _lastCursorPosition.Top);
                            SendObject(new SendInputToShellCommand(input, _processId));
                        }
                    }
                }
            }
            finally {
                _packageTransmitter.PackageReceived -= PackageTransmitterOnPackageReceived;
            }
        }

        // ----- Callbacks
        private void PackageTransmitterOnPackageReceived(object sender, Package package)
        {
            var command = _serializer.Deserialize<Command>(package.Content);
            if (command.Name == typeof(ShellOuputReceivedResponse).Name) {
                var response = _serializer.Deserialize<ShellOuputReceivedResponse>(package.Content);
                if (response.ProcessId != _processId) return;
                Console.Write(response.Output);
                _lastCursorPosition = new CursorPosition {
                    Left = Console.CursorLeft,
                    Top = Console.CursorTop
                };
            }
            else if (command.Name == typeof(ShellEndedResponse).Name) {
                var response = _serializer.Deserialize<ShellEndedResponse>(package.Content);
                if (response.ProcessId != _processId) return;
                Console.Write($"Process ended, exit code: {response.ExitCode}. Press any key to continue.");
                _end = true;
            }
        }

        // ----- Internal logic
        private void SendObject(object command)
        {
            var package = new Package(ClientId.Unknown, _target, _serializer.Serialize(command));
            _packageTransmitter.Send(package);
        }
    }
}