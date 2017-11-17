using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using LinkUs.Core.Connection;
using NFluent;
using Xunit;

namespace LinkUs.Tests
{
    public class SocketConnectionListenerShould
    {
        private readonly SocketConnectionListener _listener;

        public SocketConnectionListenerShould()
        {
            var pool = new SocketAsyncOperationPool(10);
            var factory = new SocketConnectionFactory(pool);
            _listener = new SocketConnectionListener(factory);
        }
        ~SocketConnectionListenerShould()
        {
            _listener.StopListening();
        }

        [Theory]
        [InlineData(9006, 9006)]
        public void raise_connection_established_when_socket_connects_on_same_port(int listeningPort, int connectingPort)
        {
            // Actions
            var manualResetEvent = new ManualResetEvent(false);
            SocketConnection newConnection = null;
            _listener.ConnectionEstablished += connection => {
                newConnection = connection;
                manualResetEvent.Set();
            };

            // Actors
            _listener.StartListening(new IPEndPoint(IPAddress.Any, listeningPort));
            ConnectSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), connectingPort));

            // Asserts
            manualResetEvent.WaitOne();
            Check.That(newConnection).IsNotNull();

            // Clean
            newConnection.Close();
        }

        [Theory]
        [InlineData(9001)]
        public void not_raise_connection_established_when_socket_connects_and_not_listening(int port)
        {
            // Actions
            SocketConnection newConnection = null;
            _listener.ConnectionEstablished += connection => {
                newConnection = connection;
            };

            // Actors
            Action action = () => ConnectSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));

            // Asserts
            Check.ThatCode(action)
                 .Throws<Exception>()
                 .WithMessage($"Aucune connexion n’a pu être établie car l’ordinateur cible l’a expressément refusée 127.0.0.1:{port}");
            Check.That(newConnection).IsNull();
        }

        [Theory]
        [InlineData(9002, 9003)]
        public void not_raise_connection_established_when_socket_connects_on_different_port(int listeningPort, int connectingPort)
        {
            // Actions
            SocketConnection newConnection = null;
            _listener.ConnectionEstablished += connection => {
                newConnection = connection;
            };

            // Actors
            _listener.StartListening(new IPEndPoint(IPAddress.Any, listeningPort));
            Action action = () => ConnectSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), connectingPort));

            // Asserts
            Check.ThatCode(action)
                 .Throws<Exception>()
                 .WithMessage($"Aucune connexion n’a pu être établie car l’ordinateur cible l’a expressément refusée 127.0.0.1:{connectingPort}");
            Check.That(newConnection).IsNull();
        }

        [Theory]
        [InlineData(9004)]
        public void not_raise_connection_established_when_socket_connects_and_listening_stops(int port)
        {
            // Actions
            SocketConnection newConnection = null;
            _listener.ConnectionEstablished += connection => {
                newConnection = connection;
            };

            // Actors
            _listener.StartListening(new IPEndPoint(IPAddress.Any, port));
            _listener.StopListening();
            Action action = () => ConnectSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));

            // Asserts
            Check.ThatCode(action)
                 .Throws<Exception>()
                 .WithMessage($"Aucune connexion n’a pu être établie car l’ordinateur cible l’a expressément refusée 127.0.0.1:{port}");
            Check.That(newConnection).IsNull();
        }

        [Theory]
        [InlineData(9005)]
        public void raise_connection_established_when_socket_connects_and_listening_restarts(int port)
        {
            // Actions
            var manualResetEvent = new ManualResetEvent(false);
            SocketConnection newConnection = null;
            _listener.ConnectionEstablished += connection => {
                newConnection = connection;
                manualResetEvent.Set();
            };

            // Actors
            _listener.StartListening(new IPEndPoint(IPAddress.Any, port));
            _listener.StopListening();
            _listener.StartListening(new IPEndPoint(IPAddress.Any, port));
            ConnectSocket(new IPEndPoint(IPAddress.Parse("127.0.0.1"), port));

            // Asserts
            manualResetEvent.WaitOne();
            Check.That(newConnection).IsNotNull();

            // Clean
            newConnection.Close();
        }

        // ----- Utils
        private static void ConnectSocket(EndPoint localEndPoint)
        {
            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp) {
                SendTimeout = 50,
                ReceiveTimeout = 50,
            };
            socket.Connect(localEndPoint);
        }
    }
}