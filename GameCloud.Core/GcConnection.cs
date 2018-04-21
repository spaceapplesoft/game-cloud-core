using System;
using System.Threading.Tasks;

namespace GameCloud.Core
{
    /// <summary>
    /// Game cloud client
    /// </summary>
    public class GcConnection
    {
        public IConnectionImplementation Implementation { get; }

        public GcConnection(IConnectionImplementation implementation)
        {
            Implementation = implementation;
        }

        public Task<bool> ConnectTo(string host, int port)
        {
            return Implementation.Connect(host, port);
        }

        public Task<bool> ConnectTo(string host, int port, out string error)
        {
            return Implementation.Connect(host, port, out error);
        }

        public void SendRawData(byte[] data)
        {
            Implementation.SendRawData(data);
        }
    }
}