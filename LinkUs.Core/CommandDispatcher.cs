using System;
using System.Threading.Tasks;
using LinkUs.Core.Connection;
using LinkUs.Core.Json;

namespace LinkUs.Core
{
    public class CommandDispatcher
    {
        private readonly ISerializer _serializer;
        public PackageTransmitter PackageTransmitter { get; }

        public CommandDispatcher(PackageTransmitter packageTransmitter, ISerializer serializer)
        {
            PackageTransmitter = packageTransmitter;
            _serializer = serializer;
        }

        public Task<TResponse> ExecuteAsync<TCommand, TResponse>(TCommand command, ClientId destination = null, ClientId source = null)
        {
            destination = destination ?? ClientId.Server;
            source = source ?? ClientId.Unknown;

            var commandPackage = new Package(source, destination, _serializer.Serialize(command));

            var completionSource = new TaskCompletionSource<TResponse>();
            EventHandler<Package> packageReceivedAction = (sender, responsePackage) => {
                if (Equals(responsePackage.TransactionId, commandPackage.TransactionId)) {
                    try {
                        var response = _serializer.Deserialize<TResponse>(responsePackage.Content);
                        completionSource.SetResult(response);
                    }
                    catch (Exception ex) {
                        completionSource.SetException(ex);
                    }
                }
            };
            EventHandler closedAction = (sender, args) => {
                completionSource.SetException(new Exception("Connection Closed"));
            };
            PackageTransmitter.PackageReceived += packageReceivedAction;
            PackageTransmitter.Closed += closedAction;
            PackageTransmitter.Send(commandPackage);

            return completionSource.Task.ContinueWith(task => {
                PackageTransmitter.PackageReceived -= packageReceivedAction;
                PackageTransmitter.Closed -= closedAction;
                return task.Result;
            });
        }
        public void ExecuteAsync<TCommand>(TCommand command, ClientId destination = null)
        {
            destination = destination ?? ClientId.Server;
            var content = _serializer.Serialize(command);
            var commandPackage = new Package(ClientId.Unknown, destination, content);
            PackageTransmitter.Send(commandPackage);
        }
        //private void SendPackage(Package package)
        //{
        //    var bytes = package.ToByteArray();
        //    var networkStream = _packageTransmitter.GetStream();
        //    var lengthBytes = BitConverter.GetBytes(bytes.Length);
        //    var allBytes = new byte[bytes.Length + lengthBytes.Length];
        //    Buffer.BlockCopy(lengthBytes, 0, allBytes, 0, lengthBytes.Length);
        //    Buffer.BlockCopy(bytes, 0, allBytes, lengthBytes.Length, bytes.Length);
        //    networkStream.Write(allBytes, 0, allBytes.Length);
        //}
        //private Package ReadPackage()
        //{
        //    var network = _packageTransmitter.GetStream();
        //    var lengthBytes = new byte[4];
        //    network.Read(lengthBytes, 0, lengthBytes.Length);
        //    var length = BitConverter.ToInt32(lengthBytes, 0);
        //    var buffer = new byte[200];
        //    var finalBuffer = new byte[length];
        //    var readCount = 0;
        //    do {
        //        var load = buffer.Length;
        //        if (load > length - readCount) {
        //            load = length - readCount;
        //        }
        //        var bytesReceivedCount = network.Read(finalBuffer, 0, load);
        //        readCount += bytesReceivedCount;
        //    } while (readCount < length);
        //    return Package.Parse(finalBuffer);
        //}
    }
}