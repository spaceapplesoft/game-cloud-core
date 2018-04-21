using System;
using System.Threading;

namespace GameCloud.Core.Tests.Mocks
{
    public delegate void RawDataHandler(RemoteConnection sender, byte[] data);

    public class ServerImplementationMock : IServerImplementation
    {
        private RawDataHandler _handler;

        private int _remoteIdGenerator;

        public void HandleRawData(byte[] data)
        {
        }

        public void ChangeRawDataHandler(RawDataHandler handler)
        {
            _handler = handler;
        }

        public void HandleRawData(RemoteConnection sender, byte[] data)
        {
            // If we have a custom raw data handler, invoke it
            if (_handler != null)
            {
                _handler.Invoke(sender, data);
            }
        }

        public RemoteConnection MockRemoteConnection()
        {
            return new RemoteConnection(Interlocked.Increment(ref _remoteIdGenerator));
        }
    }
}