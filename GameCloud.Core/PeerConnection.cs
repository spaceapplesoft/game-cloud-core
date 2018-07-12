namespace GameCloud.Core
{
    /// <summary>
    /// Represents connection from server to client
    /// </summary>
    public class PeerConnection
    {
        private readonly IRemoteConnectionImplementation _implementation;
        public int ConnectionId { get; }

        public PeerConnection(int connectionId, IRemoteConnectionImplementation implementation)
        {
            _implementation = implementation;
            ConnectionId = connectionId;
        }

        public void SendRawData(byte[] data)
        {
            _implementation.SendRawData(data);
        }
    }
}