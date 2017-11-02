using System;
using LinkUs.Core;

namespace LinkUs.CommandLine
{
    public class ConsoleRemoteShellController
    {
        private readonly PackageTransmitter _packageTransmitter;
        private readonly ClientId _target;
        private readonly ISerializer _serializer;
        private bool _end;
        private CursorPosition _lastCursorPosition = new CursorPosition();

        // ----- Constructor
        public ConsoleRemoteShellController(PackageTransmitter packageTransmitter, ClientId target, ISerializer serializer)
        {
            _packageTransmitter = packageTransmitter;
            _target = target;
            _serializer = serializer;
        }

        // ----- Public methods
        public void SendInputs()
        {
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
                            SendObject(new KillShellCommand());
                        }
                        else {
                            Console.SetCursorPosition(_lastCursorPosition.Left, _lastCursorPosition.Top);
                            SendObject(new SendInputToShellCommand(input));
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
            if (command.Name == typeof(ShellStartedResponse).Name) {
                Console.WriteLine("Shell started on remote host.");
            }
            else if (command.Name == typeof(ShellOuputReceivedResponse).Name) {
                var response = _serializer.Deserialize<ShellOuputReceivedResponse>(package.Content);
                Console.Write(response.Output);
                _lastCursorPosition = new CursorPosition {
                    Left = Console.CursorLeft,
                    Top = Console.CursorTop
                };
            }
            else if (command.Name == typeof(ShellEndedResponse).Name) {
                Console.Write("Process ended. Press any key to continue.");
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