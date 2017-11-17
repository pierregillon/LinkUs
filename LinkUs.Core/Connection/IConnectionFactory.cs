using System.Net.Sockets;

namespace LinkUs.Core.Connection
{
    public interface IConnectionFactory<out T> where T : IConnection
    {
        T Create(Socket socket);
    }
}