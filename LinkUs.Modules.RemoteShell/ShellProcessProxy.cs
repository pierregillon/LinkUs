using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using LinkUs.Modules.RemoteShell.Events;

namespace LinkUs.Modules.RemoteShell
{
    public class ShellProcessProxy
    {
        private readonly dynamic _bus;
        private readonly Process _shellProcess;

        // ----- Constructor
        public ShellProcessProxy(dynamic bus)
        {
            _shellProcess = NewCmdProcess();
            _bus = bus;
        }

        // ----- Public methods
        public int Start(string commandLine, string[] arguments)
        {
            if (commandLine != "cmd") {
                _shellProcess.StartInfo.Arguments = $"/C {commandLine} " + string.Join(" ", arguments);
            }
            _shellProcess.ErrorDataReceived += ShellProcessOnErrorDataReceived;
            if (_shellProcess.Start() == false) {
                throw new Exception("Unable to start the shell process.");
            }
            _shellProcess.BeginErrorReadLine();
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
                        var message = new ShellOutputReceived(textToSend, _shellProcess.Id);
                        _bus.Send(message);
                    }
                }
                _bus.Send(new ShellEnded(_shellProcess.ExitCode, _shellProcess.Id));
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
        private void ShellProcessOnErrorDataReceived(object o, DataReceivedEventArgs args)
        {
            _bus.Send(new ShellOutputReceived(args.Data, _shellProcess.Id));
        }

        // ----- Utils
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
                    RedirectStandardError = true,
                    Verb = "runas"
                },
                EnableRaisingEvents = true
            };
        }
    }
}