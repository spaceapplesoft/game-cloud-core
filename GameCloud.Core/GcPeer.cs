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

        public event GcServer.PeerEventHandler Disconnected;
        private bool _isDisconnected;

        /// <summary>
        /// If it's a virutal peer, it means that it's part of a "relay" chain
        /// </summary>
        public bool IsVirtual { get; }
        
        /// <summary>
        /// Peer, through which we're relaying this virtual peer
        /// </summary>
        private GcPeer _concretePeer;
        
        public GcPeer(int peerId, PeerConnection connection)
        {
            _writer = new NetWriter();
            _connection = connection;
            _relayedPeerIds = new Dictionary<GcConnection, int>();
            PeerId = peerId;
        }

        public GcPeer(int peerId, GcPeer concretePeer)
        {
            _writer = new NetWriter();
            _relayedPeerIds = new Dictionary<GcConnection, int>();
            PeerId = peerId;

            IsVirtual = true;
            _concretePeer = concretePeer;
        }
        
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

            if (!IsVirtual)
            {
                lock (_writer)
                {
                    GcProtocol.PackMessage(_writer, opCode, writeAction, requestId, responseId, status, null, defaultFlags);
                    data = _writer.ToArray();
                }
                _connection.SendRawData(data);
            }
            else
            {
                lock (_writer)
                {
                    GcProtocol.PackMessage(_writer, opCode, writeAction, requestId, responseId, status, PeerId, defaultFlags);
                    data = _writer.ToArray();
                }
                _concretePeer.SendRawData(data);
            }
            
        }

        internal void SendRawData(byte[] data)
        {
            if (_isDisconnected)
                return;
            
            if (!IsVirtual)
            {
                _connection.SendRawData(data);
            }
            else
            {
                _concretePeer.SendRawData(data);
            }
            
        }

        public void OnDisconnected()
        {
            _isDisconnected = true;
            Disconnected?.Invoke(this);
        }

        /// <summary>
        /// For forwarding messages.
        /// Saves information that this peer will be represented by a given peerId in a different server
        /// </summary>
        /// <param name="peerId"></param>
        /// <param name="connection"></param>
        public void SetPeerIdInRelayedServer(int peerId, GcConnection connection)
        {
            _relayedPeerIds.TryAdd(connection, peerId);
        }
    }
}