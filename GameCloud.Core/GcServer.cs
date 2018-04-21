using System.Threading.Tasks;

namespace GameCloud.Core
{
    /// <summary>
    /// Game cloud server
    /// </summary>
    public class GcServer
    {
        public IServerImplementation Implementation { get; }

        public int Port { get; private set; }

        public GcServer(IServerImplementation implementation)
        {
            Implementation = implementation;
        }

        public GcServer Start(int port)
        {
            Port = port;
            return this;
        }

        public Task<bool> RelayTo(string host, int port)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RelayTo(short opCode, string host, int port)
        {
            throw new System.NotImplementedException();
        }

        public Task<bool> RelayTo(short opCodeFrom, short opCodesTo, string host, int port)
        {
            throw new System.NotImplementedException();
        }
    }
}