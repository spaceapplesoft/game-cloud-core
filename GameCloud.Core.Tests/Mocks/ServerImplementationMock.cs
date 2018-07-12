using System;
using System.Threading;

namespace GameCloud.Core.Tests.Mocks
{
    public class ServerImplementationMock : IServerImplementation
    {
        private int _remoteIdGenerator = 1;

        public event RawDataHandler RawDataReceived;

        public void HandleRawData(PeerConnection sender, byte[] data)
        {
            RawDataReceived?.Invoke(sender, data);
        }

        public (RemoteConnectionImplementationMock, PeerConnection) MockClientToServerLink()
        {
            var remoteId = Interlocked.Increment(ref _remoteIdGenerator);
            var remoteConImplementation = new RemoteConnectionImplementationMock();
            var connection = new PeerConnection(remoteId, remoteConImplementation);


            return (remoteConImplementation, connection);
        }

        public void MockDisconnect(PeerConnection connection)
        {
            ConnectionLost?.Invoke(connection);
        }
   
        public event ConnectionEventHandler ConnectionReceived;
        public event ConnectionEventHandler ConnectionLost;

        public void MockConnectedPeer(PeerConnection connection)
        {
            ConnectionReceived?.Invoke(connection);
        }
    }
}