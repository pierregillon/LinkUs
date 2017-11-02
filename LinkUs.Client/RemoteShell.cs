using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LinkUs.Core;

namespace LinkUs.Client
{
    public class RemoteShell
    {
        private readonly PackageTransmitter _packageTransmitter;
        private readonly Package _package;
        private readonly Process _shellProcess;
        private readonly JsonSerializer _jsonSerializer = new JsonSerializer();

        // ----- Constructor
        public RemoteShell(PackageTransmitter packageTransmitter, Package package, ExecuteShellCommand executeRemoteCommand)
        {
            _packageTransmitter = packageTransmitter;
            _package = package;
            _shellProcess = NewCmdProcess();
            if (executeRemoteCommand.CommandLine != "cmd") {
                _shellProcess.StartInfo.Arguments = $"/C {executeRemoteCommand.CommandLine} " + string.Join(" ", executeRemoteCommand.Arguments);
            }
            _shellProcess.ErrorDataReceived += ShellProcessOnErrorDataReceived;
        }

        // ----- Public methods
        public Task Start()
        {
            _shellProcess.Start();
            _shellProcess.BeginErrorReadLine();

            SendToController(new ShellStartedResponse {ProcessId = _shellProcess.Id});

            return Task.Factory.StartNew(() => {
                var buffer = new char[1024];
                while (_shellProcess.StandardOutput.EndOfStream == false) {
                    var bytesReadCount = _shellProcess.StandardOutput.Read(buffer, 0, buffer.Length);
                    if (bytesReadCount > 0) {
                        var textToSend = new string(buffer, 0, bytesReadCount);
                        SendToController(new ShellOuputReceivedResponse(textToSend));
                    }
                }
                SendToController(new ShellEndedResponse(_shellProcess.ExitCode));
            });
        }
        public void Stop()
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
            SendToController(new ShellOuputReceivedResponse(dataReceivedEventArgs.Data));
        }

        // ----- Utils
        private void SendToController(object response)
        {
            var content = _jsonSerializer.Serialize(response);
            var responsePackage = _package.CreateResponsePackage(content);
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