using System.Threading.Tasks;

namespace GameCloud.Core
{
    public class GcServer
    {
        public int Port { get; }

        public GcServer(int port)
        {
            Port = port;
        }

        public GcServer Start()
        {
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