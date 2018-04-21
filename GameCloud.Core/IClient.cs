using System.Threading.Tasks;

namespace GameCloud.Core
{
    public interface IClient
    {
        void SendRawData(byte[] data);

        Task<bool> Connect(string host, int port);
        Task<bool> Connect(string host, int port, out string error);
    }
}