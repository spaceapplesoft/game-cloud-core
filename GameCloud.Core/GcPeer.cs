using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace GameCloud.Core
{
    public class GcPeer
    {
        private readonly PeerConnection _connection;
        public int PeerId { get; }

        private Dictionary<GcConnection, int> _relayedPeerIds;

        private NetWriter _writer;

        public GcServer.PeerEventHandler Disconnected;
        private bool _isDisconnected;

        public GcPeer(int peerId, PeerConnection connection)
        {
            _writer = new NetWriter();
            _connection = connection;
            _relayedPeerIds = new Dictionary<GcConnection, int>();
            PeerId = peerId;
        }

        public bool IsRelayed { get; }
        
        public int GetPeerIdInRelayedServer(GcConnection connection)
        {
            _relayedPeerIds.TryGetValue(connection, out var result);

            // Return -1 instead of 0 for missing result,
            // because it's more "meaningful"
            return result == 0 ? -1 : result;
        }
        
        public void SendBasicMessage(short opCode, Action<NetWriter> writeAction,
            int? requestId, int? responseId, ResponseStatus? status, byte defaultFlags = 0)
        {
            if (_isDisconnected)
                return;
            
            byte[] data;

            lock (_writer)
            {
                var peerId = IsRelayed ? PeerId : (int?) null;
                GcProtocol.PackMessage(_writer, opCode, writeAction, requestId, responseId, status, peerId, defaultFlags);
                data = _writer.ToArray();
            }

            _connection.SendRawData(data);
        }

        public void OnDisconnected()
        {
            _isDisconnected = true;
            Disconnected?.Invoke(this);
        }
    }
}