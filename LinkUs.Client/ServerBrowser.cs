using System;
using System.Net.Sockets;
using System.Threading;
using LinkUs.Core.Connection;

namespace LinkUs.Client
{
    public class ServerBrowser
    {
        private readonly Connector _connector;

        public ServerBrowser(Connector connector)
        {
            _connector = connector;
        }

        public IConnection SearchAvailableHost()
        {
            string host = "127.0.0.1";
            int port = 9000;

            while (true) {
                try {
                    Console.Write($"* Try to connect to host {host} on port {port} ... ");
                    var connection = _connector.Connect(host, port);
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