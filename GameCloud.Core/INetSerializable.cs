namespace GameCloud.Core
{
    public interface INetSerializable
    {
        void Serialize(NetWriter writer);

        void Deserialize(NetReader reader);
    }
}