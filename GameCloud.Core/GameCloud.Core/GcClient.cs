using System;
using System.Threading.Tasks;

namespace GameCloud.Core
{
    public class GcClient
    {
        public IClient Connection { get; }

        public GcClient(IClient connection)
        {
            Connection = connection;
        }

        public Task<bool> ConnectTo(string host, int port)
        {
            return Connection.Connect(host, port);
        }

        public Task<bool> ConnectTo(string host, int port, out string error)
        {
            return Connection.Connect(host, port, out error);
        }

        public void SendRawData(byte[] data)
        {
            Connection.SendRawData(data);
        }
    }
}