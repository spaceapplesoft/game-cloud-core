using System;
using System.Threading.Tasks;

namespace GameCloud.Core
{
    public interface IConnectionImplementation
    {
        void SendRawData(byte[] data);

        event Action<byte[]> DataReceived;
        event Action Disconnected;

        Task<bool> Connect(string host, int port);
        Task<bool> Connect(string host, int port, out string error);
        void Disconnect();
        bool IsConnected { get;}
    }
}