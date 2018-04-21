using System;
using System.Threading;
using System.Threading.Tasks;
using GameCloud.Core.Tests.Mocks;
using Xunit;

namespace GameCloud.Core.Tests
{
    public class Client_to_Server
    {
        private readonly GcServer _server;
        private readonly GcConnection _connection;

        private readonly ServerImplementationMock _serverMock;

        public Client_to_Server()
        {
            _serverMock = new ServerImplementationMock();
            _server = new GcServer(_serverMock).Start(500);

            _connection = new GcConnection(new ConnectionMock(_server, _serverMock.MockRemoteConnection()));
        }

        [Fact]
        public async Task ClientConnectsToServer()
        {
            var isConnected = await _connection.ConnectTo("127.0.0.1", _server.Port);

            Assert.True(isConnected);
        }

        [Fact]
        public async Task ClientDoesntConnectToServer()
        {
            var isConnected = await _connection.ConnectTo("127.0.0.1", 666);

            Assert.False(isConnected);
        }

        [Fact]
        public async Task ClientSendsRawDataToServer()
        {
            await _connection.ConnectTo("127.0.0.1", _server.Port);

            var data = new byte[] {1, 1, 2, 1, 1};

            var completionSource = new TaskCompletionSource<bool>();

            byte[] receivedData = null;

            // Set a custom raw data handler, so that we can intercept raw data
            _serverMock.ChangeRawDataHandler((sender, bytes) =>
            {
                // Check if all data received
                if (bytes.Length != data.Length)
                {
                    completionSource.SetResult(false);
                    return;
                }

                // Check if all the data is unchanged
                for (var i = 0; i < bytes.Length; i++)
                {
                    if (data[i] != bytes[i])
                    {
                        completionSource.SetResult(false);
                        return;
                    }
                }
                completionSource.SetResult(true);
            });

            _connection.Implementation.SendRawData(data);

            var isRawDataReceived = await completionSource.Task;

            Assert.True(isRawDataReceived);
        }
    }
}
