namespace GameCloud.Core
{
    public delegate void RawDataHandler(PeerConnection sender, byte[] data);

    public delegate void ConnectionEventHandler(PeerConnection connection);

    public interface IServerImplementation
    {
        event ConnectionEventHandler ConnectionReceived;
        event ConnectionEventHandler ConnectionLost;

        event RawDataHandler RawDataReceived;
        void HandleRawData(PeerConnection sender, byte[] data);
    }
}