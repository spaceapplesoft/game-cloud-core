using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
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
            
            _connection = TestUtils.CreateMockConnection(_serverMock);
        }

        [Fact]
        public async Task ClientConnectsToServer()
        {
            var isConnected = await _connection.ConnectTo("127.0.0.1", _server.Port);

            Assert.True(isConnected);
        }

        [Fact]
        public async Task MultipleClientsConnectToServer()
        {
            var connectionCount = 10;
            
            // ----------------------
            // Check if server gets all peers
            var peersTestSource = new TaskCompletionSource<bool>();
            var joinedPeerIds = new List<int>();
            _server.PeerJoined += peer =>
            {
                if (joinedPeerIds.Contains(peer.PeerId))
                {
                    // Same peer id was generated
                    throw new Exception("Same peer id generated");
                }
                joinedPeerIds.Add(peer.PeerId);

                if (joinedPeerIds.Count == connectionCount)
                {
                    peersTestSource.SetResult(true);
                }
            };
            
            // ----------------------
            // Check if all connections are established
            
            var connections = new List<GcConnection>();
            
            for (var i = 0; i < connectionCount; i++)
            {
                var connection = TestUtils.CreateMockConnection(_serverMock);
                connections.Add(connection);
            }

            // Wait for all connections to be established
            var results = await Task.WhenAll(connections.Select(c => c.ConnectTo("127.0.0.1", _server.Port)));

            foreach (var result in results)
            {
                Assert.True(result);
            }
            
            // Wait for all peerjoined events to be triggered
            Assert.True(await peersTestSource.Task);
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

            const int nubmerToSend = 777;
            var completionSource = new TaskCompletionSource<bool>();

            // Handle the message
            _server.SetHandler(5, message =>
            {
                message.Reader.ReadString();
                var number = message.Reader.ReadInt32();
                completionSource.SetResult(number == nubmerToSend);
                return Task.CompletedTask;
            });

            // Send the message
            _connection.Send(5, writer => writer.Write("String").Write(nubmerToSend));

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
