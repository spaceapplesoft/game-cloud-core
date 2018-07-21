using System;
using System.ComponentModel;
using System.Threading.Tasks;
using GameCloud.Core.Tests.Mocks;
using Microsoft.Extensions.Logging;
using Xunit;

namespace GameCloud.Core.Tests
{
    public class Relay
    {
        private readonly GcServer _serverA;
        private readonly GcServer _serverB;
        private readonly GcServer _serverC;
        
        private readonly ServerImplementationMock _serverAMock;
        private readonly ServerImplementationMock _serverBMock;
        private readonly ServerImplementationMock _serverCMock;

        
        public Relay()
        {
            var logger = new LoggerFactory()
                .AddConsole()
                .CreateLogger<Client_to_Server>();
            
            _serverAMock = new ServerImplementationMock();
            _serverBMock = new ServerImplementationMock();
            _serverCMock = new ServerImplementationMock();
            
            _serverA = new GcServer(_serverAMock, logger);
            _serverB = new GcServer(_serverBMock, logger);
            _serverC = new GcServer(_serverCMock, logger);
        }
        
        /// <summary>
        /// Client -> ServerA -> ServerB
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RelaySimpleMessage()
        {
            const short opCode = 10;
            var numberToSend = 777;
            
            var client = TestUtils.CreateMockConnection(_serverAMock);
            var aToB = TestUtils.CreateMockConnection(_serverBMock);

            Console.WriteLine("TEST");
            
            // -------------------------------------------
            // SETUP CONNECTIONS
            
            // Connect Client -> ServerA
            Assert.True(await client.ConnectTo("127.0.0.1", _serverA.Port));
            
            // Connect ServerA -> ServerB
            Assert.True(await aToB.ConnectTo("127.0.0.1", _serverB.Port));

            // Register a relay
            _serverA.RelayTo(opCode, aToB);
            
            var completionSource = new TaskCompletionSource<bool>();
            
            // Handle the message on server B
            _serverB.SetHandler(opCode, message =>
            {

                message.Reader.ReadString();
                var number = message.Reader.ReadInt32();
                completionSource.SetResult(number == numberToSend);
                return Task.CompletedTask;
            });
            
            // -------------------------------------------
            // ESTABLISH THE PEER

            var isEstablished = await client.EstablishPeer(TimeSpan.FromSeconds(5));
            
            Assert.True(isEstablished);
            
            // -------------------------------------------
            // SEND THE MESSAGE
            
            // Send the message
            client.Send(opCode, writer => writer.Write("Str").Write(numberToSend));

            var isDataReceived = await completionSource.Task;

            Assert.True(isDataReceived);
        }
        
        /// <summary>
        /// Client -> ServerA -> ServerB -> SerberA -> Client
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task RelayRequest()
        {
            const short opCode = 10;
            var numberToSend = 777;
            
            var client = TestUtils.CreateMockConnection(_serverAMock);
            var aToB = TestUtils.CreateMockConnection(_serverBMock);

            // -------------------------------------------
            // SETUP CONNECTIONS
            
            // Connect Client -> ServerA
            Assert.True(await client.ConnectTo("127.0.0.1", _serverA.Port));
            
            // Connect ServerA -> ServerB
            Assert.True(await aToB.ConnectTo("127.0.0.1", _serverB.Port));

            // Register a relay
            _serverA.RelayTo(opCode, aToB);
            
            
            // Handle the message on server B
            _serverB.SetHandler(opCode, message =>
            {
                message.Reader.ReadString();
                var number = message.Reader.ReadInt32();
                message.Respond(ResponseStatus.Success, w => w.Write(number));
                return Task.CompletedTask;
            });
            
            // -------------------------------------------
            // ESTABLISH THE PEER

            var isEstablished = await client.EstablishPeer(TimeSpan.FromSeconds(5));
            
            Assert.True(isEstablished);
            
            // -------------------------------------------
            // SEND THE MESSAGE
            
            // Send the message
            var response = await client.SendRequest(opCode, writer => writer.Write("Str").Write(numberToSend));

            var responseNumber = response.Reader.ReadInt32();
            
            Assert.True(responseNumber == numberToSend);
        }
        
    }
}