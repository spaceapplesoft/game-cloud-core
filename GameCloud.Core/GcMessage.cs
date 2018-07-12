using System;

namespace GameCloud.Core
{
    public class GcMessage
    {
        public byte Flags { get; private set; }
        public GcPeer Peer { get; private set; }
        public NetReader Reader { get; set; }
        public short OpCode { get; set; }
        public bool RequiresResponse { get { return RequestId >= 0 && !_responseSent; } }
        public ResponseStatus Status { get; set; }

        public int RequestId { get; set; }
        public int ResponseId { get; set; }

        private bool _responseSent;

        public GcMessage(GcPeer sender, byte flags)
        {
            Peer = sender;
            RequestId = -1;
            ResponseId = -1;
            OpCode = -1;
            Flags = flags;
        }

        /// <summary>
        /// Sends a response
        /// </summary>
        /// <param name="status"></param>
        /// <param name="writeAction"></param>
        /// <param name="channel"></param>
        public void Respond(ResponseStatus status, Action<NetWriter> writeAction)
        {
            if (!RequiresResponse)
                throw new Exception("Sender is not expecting for a response");

            Peer.SendBasicMessage(OpCode, writeAction, null, RequestId, status);
            _responseSent = true;
        }

        /// <summary>
        /// Responds with a serializable packet
        /// </summary>
        /// <param name="status"></param>
        /// <param name="serializable"></param>
        public void Respond(ResponseStatus status, INetSerializable serializable)
        {
            Respond(status, writer => writer.Write(serializable));
            _responseSent = true;
        }

        /// <summary>
        /// Responds with a string message
        /// </summary>
        /// <param name="status"></param>
        /// <param name="message"></param>
        public void Respond(ResponseStatus status, string message)
        {
            Respond(status, writer => writer.Write(message));
            _responseSent = true;
        }

        /// <summary>
        /// Sends an empty response
        /// </summary>
        /// <param name="status"></param>
        public void Respond(ResponseStatus status)
        {
            Respond(status, writer => { });
            _responseSent = true;
        }

        /// <summary>
        /// Tries to read a string from the reader,
        /// and if there's no data for the string - returns null
        /// </summary>
        public string AsString()
        {
            return AsString(null);
        }

        /// <summary>
        /// Tries to read a string from the reader,
        /// and if exception is thrown - returns a default string
        /// </summary>
        /// <param name="defaultMessage"></param>
        /// <returns></returns>
        public string AsString(string defaultMessage)
        {
            try
            {
                var prevPos = Reader.Position;
                var str = Reader.ReadString();
                Reader.SetPosition(prevPos);

                return str;
            }
            catch
            {
                return defaultMessage;
            }
        }
    }
}