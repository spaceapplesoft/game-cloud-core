using System;

namespace GameCloud.Core
{
    public interface IRemoteConnectionImplementation
    {
        void SendRawData(byte[] data);
    }
}