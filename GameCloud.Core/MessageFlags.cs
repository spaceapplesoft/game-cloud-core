namespace GameCloud.Core
{
    public class MessageFlags
    {
        public const byte
            InternalMessage = (1 << 0),
            Request = (1 << 1),
            Response = (1 << 2),
            RelayedForward = (1 << 3),
            RelayedBackward = (1 << 4),
            PaddedPeerId = (1 << 5);
    }
}