namespace GameCloud.Core
{
    public interface IServerImplementation
    {
        void HandleRawData(RemoteConnection sender, byte[] data);
    }
}