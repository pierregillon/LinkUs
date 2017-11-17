using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public class SocketConnectionFactory : IConnectionFactory<SocketConnection>
    {
        private readonly SocketAsyncOperationPool _socketAsyncOperationPool;

        public SocketConnectionFactory(SocketAsyncOperationPool socketAsyncOperationPool)
        {
            _socketAsyncOperationPool = socketAsyncOperationPool;
        }

        public SocketConnection Create(Socket socket)
        {
            return new SocketConnection(_socketAsyncOperationPool, socket);
        }
    }
}