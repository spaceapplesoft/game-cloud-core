using System;
using System.Threading.Tasks;

namespace GameCloud.Core
{
    public delegate Task MessageHandler(GcMessage message);

    public delegate GcPeer IndirectPeerFinder(int peerId);

    public static class GcProtocol
    {
        /// <summary>
        /// Packs the message into <see cref="NetWriter"/>
        /// </summary>
        /// <param name="writer"></param>
        /// <param name="writeAction"></param>
        /// <param name="opCode"></param>
        /// <param name="requestId"></param>
        /// <param name="responseId"></param>
        /// <param name="status"></param>
        /// <param name="defaultFlags"></param>
        public static void PackMessage(NetWriter writer, short opCode, Action<NetWriter> writeAction,  
            int? requestId, int? responseId, ResponseStatus? status, int? peerId, byte defaultFlags = 0)
        {
            // Move to front of the buffer
            writer.SetPosition(0);

            var flags = defaultFlags;
            if (requestId.HasValue) flags |= MessageFlags.Request;
            if (responseId.HasValue) flags |= MessageFlags.Response;
            if (peerId.HasValue) flags |= MessageFlags.PaddedPeerId;

            // Write flags
            writer.Write(flags);

            // Write opCode
            writer.Write(opCode);

            // Add peer id if it's provided
            if (peerId.HasValue)
            {
                writer.Write(peerId.Value);
            }

            // Write *optional* data
            if (requestId.HasValue) writer.Write(requestId.Value);
            if (responseId.HasValue)
            {
                writer.Write(responseId.Value);
                writer.Write((byte)(status ?? ResponseStatus.Default));
            }

            // Write message content
            writeAction(writer);
        }

        public static GcMessage ParseMessage(GcPeer peer, byte[] data)
        {
            var reader = new NetReader(data);

            var flags = reader.ReadByte();
            var opCode = reader.ReadInt16();

            // Read peer id if it's provided
            if ((flags & MessageFlags.PaddedPeerId) > 0)
            {
                // Read padded peer id
                var peerId = reader.ReadInt32();
            }

            var msg = new GcMessage(peer, flags)
            {
                OpCode = opCode,
                Reader = reader
            };

            if ((flags & MessageFlags.Request) > 0)
            {
                var requestId = reader.ReadInt32();
                msg.RequestId = requestId;
            }

            if ((flags & MessageFlags.Response) > 0)
            {
                var responseId = reader.ReadInt32();
                var responseStatus = (ResponseStatus)reader.ReadByte();
                msg.ResponseId = responseId;
                msg.Status = responseStatus;
            }

            return msg;
        }
    }
}