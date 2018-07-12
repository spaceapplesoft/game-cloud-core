using GameCloud.Core.Tests.Mocks;

namespace GameCloud.Core.Tests
{
    public class TestUtils
    {
        public static GcConnection CreateMockConnection(ServerImplementationMock serverMock)
        {
            var clientToServer = serverMock.MockClientToServerLink();
            // Create a link from client to server
            var connectionMock = new ConnectionMock(serverMock, clientToServer.Item2);
            // Create a link from server to client
            clientToServer.Item1.SetConnectionMock(connectionMock);
            return new GcConnection(connectionMock);
        }
    }
}