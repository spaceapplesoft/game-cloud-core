using System.Threading.Tasks;

namespace GameCloud.Core.Tests.Mocks
{
    public class ClientMock : IClient
    {
        private readonly GcServer _server;

        public ClientMock(GcServer server)
        {
            _server = server;
        }

        public void SendRawData(byte[] data)
        {
            throw new System.NotImplementedException();
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