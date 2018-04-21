using System;
using System.Threading.Tasks;
using GameCloud.Core.Tests.Mocks;
using Xunit;

namespace GameCloud.Core.Tests
{
    public class Client_to_Server
    {
        private readonly GcServer _server;
        private readonly GcClient _client;

        public Client_to_Server()
        {
            _server = new GcServer(500).Start();
            _client = new GcClient(new ClientMock(_server));
        }

        [Fact]
        public async Task ClientConnectsToServer()
        {
            var isConnected = await _client.ConnectTo("127.0.0.1", _server.Port);

            Assert.True(isConnected);
        }

        [Fact]
        public async Task ClientDoesntConnectToServer()
        {
            var isConnected = await _client.ConnectTo("127.0.0.1", 666);

            Assert.False(isConnected);
        }

        [Fact]
        public async Task ClientSendsRawDataToServer()
        {
            await _client.ConnectTo("127.0.0.1", _server.Port);

            var data = new byte[] {1, 1, 2, 1, 1};

            _client.Connection.SendRawData(data);
        }
    }
}
