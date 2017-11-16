using System;
using System.Net.Sockets;
using System.Threading;
using LinkUs.Core.Connection;

namespace LinkUs.Client
{
    public class ServerBrowser
    {
        public IConnection SearchAvailableHost()
        {
            string host = "127.0.0.1";
            int port = 9000;

            var operationPool = new SocketAsyncOperationPool(10);
            var connection = new SocketConnection(operationPool);
            while (true) {
                try {
                    Console.Write($"* Try to connect to host {host} on port {port} ... ");
                    connection.Connect(host, port);
                    Console.WriteLine("[SUCCESS]");
                    return connection;
                }
                catch (SocketException) {
                    Console.WriteLine("[FAILED]");
                    Thread.Sleep(1000);
                }
            }
        }
    }
}