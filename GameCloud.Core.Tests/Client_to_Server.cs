using System;
using System.Threading;
using System.Threading.Tasks;
using GameCloud.Core.Tests.Mocks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
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
            var logger = new LoggerFactory()
                .AddConsole()
                .CreateLogger<Client_to_Server>();

            _serverMock = new ServerImplementationMock();
            _server = new GcServer(_serverMock, logger).Start(500);

            var clientToServer = _serverMock.MockClientToServerLink();
            // Create a link from client to server
            var connectionMock = new ConnectionMock(_serverMock, clientToServer.Item2);
            // Create a link from server to client
            clientToServer.Item1.SetConnectionMock(connectionMock);
            _connection = new GcConnection(connectionMock);
            
        }

        [Fact]
        public async Task ClientConnectsToServer()
        {
            var isConnected = await _connection.ConnectTo("127.0.0.1", _server.Port);

            Assert.True(isConnected);
        }

        [Fact]
        public async Task ClientSendsRawDataToServer()
        {
            await _connection.ConnectTo("127.0.0.1", _server.Port);

            var data = new byte[] {1, 1, 2, 1, 1};

            var completionSource = new TaskCompletionSource<bool>();

            // Handle raw data
            _serverMock.RawDataReceived += (sender, bytes) =>
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
            };

            _connection.Implementation.SendRawData(data);

            var isRawDataReceived = await completionSource.Task;

            Assert.True(isRawDataReceived);
        }

        [Fact]
        public async Task ClientSendsSimpleMessage()
        {
            await _connection.ConnectTo("127.0.0.1", _server.Port);

            var completionSource = new TaskCompletionSource<bool>();

            // Handle the message
            _server.SetHandler(5, message =>
            {
                completionSource.SetResult(true);
                return Task.CompletedTask;
            });

            // Send the message
            _connection.Send(5, writer => writer.Write("String").Write(10));

            var isDataReceived = await completionSource.Task;

            Assert.True(isDataReceived);
        }

        [Fact]
        public async Task ClientSendsRequestMessage()
        {
            await _connection.ConnectTo("127.0.0.1", _server.Port);

            // Handle the message
            _server.SetHandler(5, message =>
            {
                //Respond not finished, GcConnection.Handle message not finished
                message.Respond(ResponseStatus.Success, w => w.Write(15));
                return Task.CompletedTask;
            });

            // Send the message
            var response = await _connection.SendRequest(5, writer => writer.Write("String").Write(10));

            Assert.True(response.Status == ResponseStatus.Success);
            Assert.True(response.Reader.ReadInt32() == 15);
        }

        [Fact]
        public async Task ServerReceivesEventAboutConnectedPeer()
        {
            var completionSource = new TaskCompletionSource<bool>();

            _server.PeerJoined += peer =>
            {
                completionSource.SetResult(true);
            };
            
            await _connection.ConnectTo("127.0.0.1", _server.Port);

            Assert.True(await completionSource.Task);
        }
        
        [Fact]
        public async Task ServerReceivesEventAboutDisconnectedPeer()
        {
            var completionSource = new TaskCompletionSource<bool>();

            _server.PeerLeft += peer =>
            {
                completionSource.SetResult(true);
            };
            
            await _connection.ConnectTo("127.0.0.1", _server.Port);

            _connection.Disconnect();
            
            Assert.True(await completionSource.Task);
        }
    }
}
