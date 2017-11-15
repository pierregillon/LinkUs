using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LinkUs.Core.Connection;
using LinkUs.Tests.Helpers;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class SocketConnectionShould
    {
        private readonly byte[] A_MESSAGE = { 1, 2, 3 };
        private readonly byte[] ANOTHER_MESSAGE = { 4, 5, 6 };
        private readonly byte[] BIG_MESSAGE = BuildBigMessage(1024 * 2 + 500);

        [Fact]
        public void transfert_simple_message_to_another_connection()
        {
            using (var interconnectedConnections = Get2InterconnectedSocketConnections()) {
                // Actors
                var dataReceivedList = new List<byte[]>();
                interconnectedConnections.Client1.DataReceived += data => {
                    dataReceivedList.Add(data);
                    interconnectedConnections.SetOperationCompleted();
                };

                // Actions
                interconnectedConnections.Client2.SendAsync(A_MESSAGE);

                // Asserts
                interconnectedConnections.WaitForOperation();
                Check.That(dataReceivedList).HasSize(1);
                Check.That(dataReceivedList[0]).ContainsExactly(A_MESSAGE);
            }
        }

        [Fact]
        public void transfert_big_message_to_another_connection()
        {
            using (var interconnectedConnections = Get2InterconnectedSocketConnections()) {
                // Actors
                var dataReceivedList = new List<byte[]>();
                interconnectedConnections.Client1.DataReceived += data => {
                    dataReceivedList.Add(data);
                    interconnectedConnections.SetOperationCompleted();
                };

                // Actions
                interconnectedConnections.Client2.SendAsync(BIG_MESSAGE);

                // Asserts
                interconnectedConnections.WaitForOperation();
                Check.That(dataReceivedList).HasSize(1);
                Check.That(dataReceivedList[0]).ContainsExactly(BIG_MESSAGE);
            }
        }

        [Fact]
        public void transfert_2_messages_to_another_connection()
        {
            using (var interconnectedConnections = Get2InterconnectedSocketConnections()) {
                // Actors
                var dataReceivedList = new List<byte[]>();
                interconnectedConnections.Client1.DataReceived += data => {
                    dataReceivedList.Add(data);
                    if (dataReceivedList.Count == 2) {
                        interconnectedConnections.SetOperationCompleted();
                    }
                };

                // Actions
                interconnectedConnections.Client2.SendAsync(A_MESSAGE);
                interconnectedConnections.Client2.SendAsync(ANOTHER_MESSAGE);

                // Asserts
                interconnectedConnections.WaitForOperation();
                Check.That(dataReceivedList).HasSize(2);
                Check.That(dataReceivedList[0]).ContainsExactly(A_MESSAGE);
                Check.That(dataReceivedList[1]).ContainsExactly(ANOTHER_MESSAGE);
            }
        }

        [Fact]
        public void transfert_2_messages_merged_in_network_stream_to_another_connection()
        {
            using (var interconnectedConnections = Get2InterconnectedSocketConnections()) {
                // Actors
                var dataReceivedList = new List<byte[]>();
                interconnectedConnections.Client1.DataReceived += data => {
                    dataReceivedList.Add(data);
                    Thread.Sleep(10); // wait for processing, merging 2nd and 3rd messages in networks stream
                    if (dataReceivedList.Count == 3) {
                        interconnectedConnections.SetOperationCompleted();
                    }
                };

                // Actions
                interconnectedConnections.Client2.SendAsync(A_MESSAGE);
                interconnectedConnections.Client2.SendAsync(ANOTHER_MESSAGE);
                interconnectedConnections.Client2.SendAsync(A_MESSAGE);

                // Asserts
                interconnectedConnections.WaitForOperation();
                Check.That(dataReceivedList).HasSize(3);
                Check.That(dataReceivedList[0]).ContainsExactly(A_MESSAGE);
                Check.That(dataReceivedList[1]).ContainsExactly(ANOTHER_MESSAGE);
                Check.That(dataReceivedList[2]).ContainsExactly(A_MESSAGE);
            }
        }

        [Fact]
        public void fully_receive_message_when_network_send_byte_one_after_one()
        {
            using (var networkSimulationSample = GetNetworkSimulationSample()) {
                // Actors
                byte[] header = { 3, 0, 0, 0 }, dataToSend = { 1, 2, 3 };
                var message = header.Concat(dataToSend).ToArray();

                var dataReceived = new byte[0];
                networkSimulationSample.SocketConnection.DataReceived += bytes => {
                    dataReceived = bytes;
                    networkSimulationSample.SetOperationCompleted();
                };

                // Actions
                for (var i = 0; i < message.Length; i++) {
                    networkSimulationSample.NetworkSimulationClient.Send(message.Skip(i).Take(1).ToArray());
                    Thread.Sleep(1);
                }

                // Asserts
                networkSimulationSample.WaitForOperation();
                Check.That(dataReceived).ContainsExactly(dataToSend);
            }
        }

        [Fact]
        public void fully_receive_message_when_header_is_truncated_between_2_messages()
        {
            using (var networkSimulationSample = GetNetworkSimulationSample()) {
                // Actors
                byte[] header = { 3, 0, 0, 0 }, dataToSend = { 1, 2, 3 };
                var message = header.Concat(dataToSend).ToArray();

                var dataReceived = new byte[0];
                networkSimulationSample.SocketConnection.DataReceived += bytes => {
                    dataReceived = bytes;
                    networkSimulationSample.SetOperationCompleted();
                };

                // Actions
                networkSimulationSample.NetworkSimulationClient.Send(message.Take(2).ToArray());
                Thread.Sleep(10);
                networkSimulationSample.NetworkSimulationClient.Send(message.Skip(2).ToArray());

                // Asserts
                networkSimulationSample.WaitForOperation();
                Check.That(dataReceived).ContainsExactly(dataToSend);
            }
        }

        [Fact]
        public void raise_closed_event_when_remote_host_initiate_closing_the_connection()
        {
            using (var connectedSockets = Get2InterconnectedSocketConnections()) {
                // Actor
                var disconnected = false;
                connectedSockets.Client1.Closed += () => {
                    disconnected = true;
                    connectedSockets.SetOperationCompleted();
                };

                // Action
                connectedSockets.Client2.Close();

                // Asserts
                connectedSockets.WaitForOperation();
                Check.That(disconnected).IsTrue();
            }
        }

        [Fact]
        public void raise_closed_event_when_initiate_closing_the_connection()
        {
            using (var connectedSockets = Get2InterconnectedSocketConnections()) {
                // Actor
                var disconnected = false;
                connectedSockets.Client1.Closed += () => {
                    disconnected = true;
                    connectedSockets.SetOperationCompleted();
                };

                // Action
                connectedSockets.Client1.Close();

                // Asserts
                connectedSockets.WaitForOperation();
                Check.That(disconnected).IsTrue();
            }
        }

        // ----- Utils
        private static ConnectedSocketConnectionSample Get2InterconnectedSocketConnections()
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
            return new ConnectedSocketConnectionSample { Client2 = client, Client1 = server };
        }
        private static NetworkSimulationSample GetNetworkSimulationSample()
        {
            SocketConnection server = null;
            var client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

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
            return new NetworkSimulationSample { NetworkSimulationClient = client, SocketConnection = server };
        }
        private static byte[] BuildBigMessage(int length)
        {
            var random = new Random((int) DateTime.Now.Ticks);
            var bytes = new byte[length];
            for (var i = 0; i < length; i++) {
                bytes[i] = (byte) random.Next(255);
            }
            return bytes;
        }
    }
}