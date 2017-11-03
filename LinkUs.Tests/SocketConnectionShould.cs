using System.Collections.Generic;
using System.Net;
using System.Threading;
using LinkUs.Core;
using LinkUs.Core.Connection;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class SocketConnectionShould
    {
        [Fact]
        public void transfert_simple_data()
        {
            // Actors
            var manualSetEvent = new ManualResetEvent(false);
            var dataReceivedList = new List<byte[]>();
            var connectedSockets = GetConnectedSockets();
            connectedSockets.Server.DataReceived += data => {
                dataReceivedList.Add(data);
                manualSetEvent.Set();
            };

            // Actions
            var message = new[] { (byte)1, (byte)2, (byte)3 };
            connectedSockets.Client.SendAsync(message);

            // Asserts
            manualSetEvent.WaitOne();
            Check.That(dataReceivedList).HasSize(1);
            Check.That(dataReceivedList[0]).ContainsExactly(message);
            connectedSockets.Close();
        }

        [Fact]
        public void double_send_are_correctly_read_by_fast_absorption()
        {
            // Actors
            var manualSetEvent = new ManualResetEvent(false);
            var dataReceivedList = new List<byte[]>();
            var connectedSockets = GetConnectedSockets();
            connectedSockets.Server.DataReceived += data => {
                dataReceivedList.Add(data);
                if (dataReceivedList.Count == 2) {
                    manualSetEvent.Set();
                }
            };

            // Actions
            var message1 = new[] { (byte)1, (byte)2, (byte)3 };
            var message2 = new[] { (byte)4, (byte)5, (byte)6 };

            connectedSockets.Client.SendAsync(message1);
            connectedSockets.Client.SendAsync(message2);

            // Asserts
            manualSetEvent.WaitOne();
            Check.That(dataReceivedList).HasSize(2);
            Check.That(dataReceivedList[0]).ContainsExactly(message1);
            Check.That(dataReceivedList[1]).ContainsExactly(message2);
            connectedSockets.Close();
        }

        [Fact]
        public void merge_send_when_processing_previous_one()
        {
            // Actors
            var manualSetEvent = new ManualResetEvent(false);
            var dataReceivedList = new List<byte[]>();
            var connectedSockets = GetConnectedSockets();
            connectedSockets.Server.DataReceived += data => {
                dataReceivedList.Add(data);
                Thread.Sleep(100); // wait for processing, merging 2nd and 3rd messages
                if (dataReceivedList.Count == 3) {
                    manualSetEvent.Set();
                }
            };

            // Actions
            var message1 = new[] { (byte)1, (byte)2, (byte)3 };
            var message2 = new[] { (byte)4, (byte)5, (byte)6 };
            var message3 = new[] { (byte)7, (byte)8, (byte)9 };

            connectedSockets.Client.SendAsync(message1);
            connectedSockets.Client.SendAsync(message2);
            connectedSockets.Client.SendAsync(message3);

            // Asserts
            manualSetEvent.WaitOne();
            Check.That(dataReceivedList).HasSize(3);
            Check.That(dataReceivedList[0]).ContainsExactly(message1);
            Check.That(dataReceivedList[1]).ContainsExactly(message2);
            Check.That(dataReceivedList[2]).ContainsExactly(message3);
            connectedSockets.Close();
        }

        // ----- Utils
        private ConnectedSocketSample GetConnectedSockets()
        {
            SocketConnection server = null;
            var client = new SocketConnection();

            var listener = new SocketConnectionListener(new IPEndPoint(IPAddress.Any, 9000));
            listener.ConnectionEstablished += connection => {
                server = connection;
                listener.StopListening();
            };
            listener.StartListening();

            client.Connect("127.0.0.1", 9000);
            while (server == null) {
                Thread.Sleep(10);
            }
            return new ConnectedSocketSample {Client = client, Server = server};
        }
    }

    public class ConnectedSocketSample
    {
        public SocketConnection Server { get; set; }
        public SocketConnection Client { get; set; }
        public void Close()
        {
            Server.Close();
            Client.Close();
        }
    }
}
