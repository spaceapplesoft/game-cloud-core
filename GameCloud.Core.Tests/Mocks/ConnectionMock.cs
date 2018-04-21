using System.Threading.Tasks;

namespace GameCloud.Core.Tests.Mocks
{
    public class ConnectionMock : IConnectionImplementation
    {
        private readonly GcServer _server;

        private RemoteConnection _connection;

        /// <summary>
        /// </summary>
        /// <param name="server">Server, to which we'll fake a connection</param>
        /// <param name="connection">Connection, which will be the fake connection to the server</param>
        public ConnectionMock(GcServer server, RemoteConnection connection)
        {
            _server = server;
            _connection = connection;
        }

        public void SendRawData(byte[] data)
        {
            _server.Implementation.HandleRawData(_connection, data);
        }

        public Task<bool> Connect(string host, int port)
        {
            string error = null;
            return Connect(host, port, out error);
        }

        public Task<bool> Connect(string host, int port, out string error)
        {
            error = null;

            // If port doesn't match - don't connect
            if (_server.Port != port)
                return Task.FromResult(false);

            return Task.FromResult(true);
        }
    }
}