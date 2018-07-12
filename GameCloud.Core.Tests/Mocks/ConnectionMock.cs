using System;
using System.Threading.Tasks;

namespace GameCloud.Core.Tests.Mocks
{
    public class ConnectionMock : IConnectionImplementation
    {
        private readonly ServerImplementationMock _server;

        private PeerConnection _connection;

        public event Action<byte[]> DataReceived;
        public event Action Disconnected;

        /// <summary>
        /// </summary>
        /// <param name="server">Server, to which we'll fake a connection</param>
        /// <param name="connection">Connection, which will be the fake connection to the server</param>
        public ConnectionMock(ServerImplementationMock server, PeerConnection connection)
        {
            _server = server;
            _connection = connection;
        }

        public void SendRawData(byte[] data)
        {
            _server.HandleRawData(_connection, data);
        }

        public void HandleRawData(byte[] data)
        {
            DataReceived?.Invoke(data);
        }

        public Task<bool> Connect(string host, int port)
        {
            string error = null;
            return Connect(host, port, out error);
        }

        public Task<bool> Connect(string host, int port, out string error)
        {
            error = null;

            _server.MockConnectedPeer(_connection);
            
            return Task.FromResult(true);
        }

        public void Disconnect()
        {
            _server.MockDisconnect(_connection);
        }
    }
}