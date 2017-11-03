using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LinkUs.Core;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;
using LinkUs.Core.Shell;
using LinkUs.Core.Shell.Commands;
using LinkUs.Core.Shell.Events;

namespace LinkUs.Client
{
    public class RemoteShell
    {
        private readonly PackageTransmitter _packageTransmitter;
        private readonly Package _package;
        private readonly Process _shellProcess;
        private readonly JsonSerializer _jsonSerializer = new JsonSerializer();

        // ----- Constructor
        public RemoteShell(PackageTransmitter packageTransmitter, Package package, StartShellCommand startRemoteCommand)
        {
            _packageTransmitter = packageTransmitter;
            _package = package;
            _shellProcess = NewCmdProcess();
            if (startRemoteCommand.CommandLine != "cmd") {
                _shellProcess.StartInfo.Arguments = $"/C {startRemoteCommand.CommandLine} " + string.Join(" ", startRemoteCommand.Arguments);
            }
            _shellProcess.ErrorDataReceived += ShellProcessOnErrorDataReceived;
        }

        // ----- Public methods
        public int Start()
        {
            if (_shellProcess.Start() == false) {
                throw new Exception("Unable to start the shell process.");
            }
            _shellProcess.BeginErrorReadLine();

            var content = _jsonSerializer.Serialize(new ShellStarted { ProcessId = _shellProcess.Id });
            var responsePackage = _package.CreateResponsePackage(content);
            _packageTransmitter.Send(responsePackage);

            return _shellProcess.Id;
        }
        public Task ReadOutputAsync()
        {
            return Task.Factory.StartNew(() => {
                var buffer = new char[1024];
                while (_shellProcess.StandardOutput.EndOfStream == false) {
                    var bytesReadCount = _shellProcess.StandardOutput.Read(buffer, 0, buffer.Length);
                    if (bytesReadCount > 0) {
                        var textToSend = new string(buffer, 0, bytesReadCount);
                        SendToController(new ShellOutputReceived(textToSend, _shellProcess.Id));
                    }
                }
                SendToController(new ShellEnded(_shellProcess.ExitCode, _shellProcess.Id));
            });
        }
        public void Kill()
        {
            _shellProcess.Kill();
            _shellProcess.WaitForExit();
        }
        public void Write(string input)
        {
            _shellProcess.StandardInput.Write(input);
        }

        // ----- Callbacks
        private void ShellProcessOnErrorDataReceived(object o, DataReceivedEventArgs dataReceivedEventArgs)
        {
            SendToController(new ShellOutputReceived(dataReceivedEventArgs.Data, _shellProcess.Id));
        }

        // ----- Utils
        private void SendToController(object response)
        {
            var content = _jsonSerializer.Serialize(response);
            var responsePackage = new Package(_package.Destination, _package.Source, content);
            _packageTransmitter.Send(responsePackage);
        }
        private static Process NewCmdProcess()
        {
            return new Process {
                StartInfo = {
                    FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
                    Arguments = "",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    ErrorDialog = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                },
                EnableRaisingEvents = true
            };
        }
    }
}