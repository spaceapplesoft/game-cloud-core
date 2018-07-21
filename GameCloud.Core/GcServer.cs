using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GameCloud.Core.Utils.Conversion;
using Microsoft.Extensions.Logging;

namespace GameCloud.Core
{
    /// <summary>
    /// Game cloud server
    /// </summary>
    public class GcServer
    {
        public delegate void PeerEventHandler(GcPeer peer);
        
        private readonly ILogger _logger;
        public IServerImplementation Implementation { get; }

        public int Port { get; private set; }

        private readonly Dictionary<short, MessageHandler> _messageHandlers;

        private readonly ConcurrentDictionary<int, GcPeer> _peers;
        private readonly ConcurrentDictionary<int, GcPeer> _peersByConnectionId;
        private readonly ConcurrentDictionary<int, GcConnection> _relayConnections;

        public event PeerEventHandler PeerJoined;
        public event PeerEventHandler PeerLeft;
        
        private int _peerIdGenerator = 1;

        public GcServer(IServerImplementation implementation, ILogger logger)
        {
            _logger = logger;
            var fact = new LoggerFactory();
            fact.CreateLogger<GcServer>();
            _peers = new ConcurrentDictionary<int, GcPeer>();
            _peersByConnectionId = new ConcurrentDictionary<int, GcPeer>();
            _relayConnections = new ConcurrentDictionary<int, GcConnection>();

            _messageHandlers = new Dictionary<short, MessageHandler>();
            Implementation = implementation;

            implementation.RawDataReceived += OnRawDataReceived;
            implementation.ConnectionReceived += OnConnectionReceived;
            implementation.ConnectionLost += OnConnectionLost;
            
            SetHandler((short) InternalOpCodes.EstablishPeer, HandleEstablishPeer);
        }

        private void OnConnectionReceived(PeerConnection connection)
        {
            var peerId = Interlocked.Increment(ref _peerIdGenerator);
            var peer = new GcPeer(peerId, connection);

            _peersByConnectionId.TryAdd(connection.ConnectionId, peer);
            _peers.TryAdd(peerId, peer);
            
            PeerJoined?.Invoke(peer);
        }

        private void OnConnectionLost(PeerConnection connection)
        {
            _peersByConnectionId.TryRemove(connection.ConnectionId, out var peer);

            if (peer == null)
                return;

            _peers.TryRemove(peer.PeerId, out var removedPeer);

            peer.OnDisconnected();
            
            PeerLeft?.Invoke(peer);
        }

        public GcServer Start(int port)
        {
            Port = port;
            return this;
        }

        private void OnRawDataReceived(PeerConnection sender, byte[] data)
        {
            // If the message is empty - ignore
            if (data.Length < 0)
                return;

            try
            {
                var flags = data[0];

                if ((flags & MessageFlags.InternalMessage) > 0)
                {
                    HandleInternalMessage(sender, data);
                    return;
                }

                // 1. Get the peer
                GcPeer peer = null;
                if ((flags & MessageFlags.PaddedPeerId) > 0)
                {
                    // There's a peer id within a message, which means that this message
                    // was relayed from somewhere, and we need to use an "indirect" peer
                    var peerId = EndianBitConverter.Little.ToInt32(data, 3);

                    if (peerId <= 0)
                    {
                        // This was just an empty padding, ignore it, use a direct peer
                        _peersByConnectionId.TryGetValue(sender.ConnectionId, out peer);
                    }
                    else
                    {
                        // Get a peer with provided peer id
                        _peers.TryGetValue(peerId, out peer);
                    }
                }
                else
                {
                    _peersByConnectionId.TryGetValue(sender.ConnectionId, out peer);
                }

                // Received a message from connection which has no direct peer object
                if (peer == null)
                {
                    _logger.LogWarning("Received a message from a source which doesn't have a peer");
                    return;
                }

                // 2. Handle relaying of messages
                if (_relayConnections.Count != 0)
                {
                    var opCode = EndianBitConverter.Little.ToInt16(data, 1);
                    GcConnection connectionToRelay;
                    _relayConnections.TryGetValue(opCode, out connectionToRelay);

                    // If connection to relay for this opcode exists, it means a message 
                    // needs to be relayed
                    if (connectionToRelay != null)
                    {
                        // Write relayed peer id into the messages peerId padding
                        var relayedId = peer.GetPeerIdInRelayedServer(connectionToRelay);
                        
                        // Ignore if peer doesn't have an established peer id
                        if (relayedId < 0)
                            return;

                        // TODO Do this only if there's a peerId padding, otherwise - reconstruct the whole thing
                        // Add a peer id 
                        EndianBitConverter.Little.CopyBytes(relayedId, data, 3);

                        // Pass the data
                        connectionToRelay.Implementation.SendRawData(data);
                        return;
                    }
                }

                // 3. Generate the message object
                var message = GcProtocol.ParseMessage(peer, data);

                HandleMessage(message);
            }
            catch (Exception e)
            {
                _logger.LogError("Exception while handling received data", e);
            }
        }

        private void HandleMessage(GcMessage message)
        {
            MessageHandler handler;
            _messageHandlers.TryGetValue(message.OpCode, out handler);

            if (handler == null && message.RequiresResponse)
            {
                // No handler, but message requires a response
                message.Respond(ResponseStatus.NotHandled, "Not handled");
                return;
            }

            // No handler, but don't need to respond
            if (handler == null)
                return;

            Task.Run(async () =>
            {
                try
                {
                    await handler.Invoke(message);
                }
                catch (Exception e)
                {
                    _logger.LogError("Exception while handling a request", e);

                    try
                    {
                        if (message.RequiresResponse)
                        {
                            message.Respond(ResponseStatus.Error, "Internal error");
                        }
                    }
                    catch (Exception responseException)
                    {
                        _logger.LogError("Exception while trying to respond after " +
                                         "catching an exception", responseException);
                    }
                }
            });
        }
        
        private async Task HandleEstablishPeer(GcMessage message)
        {
            var peerId = message.Reader.ReadInt32();
            var isDirectMessage = peerId < 0;
            GcPeer peer = null;
            
            if (isDirectMessage)
            {
                // This is a direct message from client
                // who can't know his peer id
                peerId = message.Peer.PeerId;
                peer = message.Peer;
            }
            else
            {
                // This is a message that was already relayed at least once
                // We need to create a virtual peer which will be used in this server
                var vPeer = new GcPeer(Interlocked.Increment(ref _peerIdGenerator), message.Peer);
                
                _peers.TryAdd(vPeer.PeerId, vPeer);
                PeerJoined?.Invoke(vPeer);
                
                // Add the newly created peer id
                peerId = vPeer.PeerId;
                peer = vPeer;
            }
            
            // Send messages to all of the relayed servers to establish a peer in them
            var connections = _relayConnections.Values.Distinct().Where(c => c.IsConnected);

            var allSuccessful = true;
            
            // Do it "linearly" (one by one) because it's a lot easier to handle
            foreach (var connection in connections)
            {
                var response =
                    await connection.SendRequest((short) InternalOpCodes.EstablishPeer, w => w.Write(peerId));
                
                if (response.Status != ResponseStatus.Success)
                {
                    _logger.LogWarning("Failed to establish a peer in connection: " + connection);
                    allSuccessful = false;
                    continue;
                }
                
                var assignedPeerId = response.Reader.ReadInt32();

                // For forwarding messages
                peer.SetPeerIdInRelayedServer(assignedPeerId, connection);
                
                // For "backwarding" messages
                connection.SaveRelayedPeer(assignedPeerId, peer);
            }

            if (isDirectMessage)
            {
                message.Respond(allSuccessful ? ResponseStatus.Success : ResponseStatus.Failed);
            }
            else
            {
                message.Respond(ResponseStatus.Success, w => w.Write(peerId));
            }
            
        }

        /// <summary>
        /// Override if you want a fine-control of how to handle the messages
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="data"></param>
        protected virtual void HandleInternalMessage(PeerConnection sender, byte[] data)
        {
            
        }

        public void RelayTo(short opCode, GcConnection connection)
        {
            if (!connection.IsConnected)
                throw new Exception("Cannot relay to a connection which is closed");

            _relayConnections.TryAdd(opCode, connection);

            connection.InterceptIncomingData(data =>
            {
                var flags = data[0];
                
                // There's no peer id
                if ((flags & MessageFlags.PaddedPeerId) <= 0)
                    return false;
                
                var peerId = EndianBitConverter.Little.ToInt32(data, 3);
                
                var peer = connection.GetRelayedPeer(peerId);
                
                // Overide the peerId of the message
                // If it's not a virtual peer, but a direct one - 
                // cleanup the peer id so that direct client doesn't know his id
                EndianBitConverter.Little.CopyBytes(peer.IsVirtual ? peer.PeerId : -1, data, 3);
                
                peer.SendRawData(data);
                
                return true;
            });
        }

        public void SetHandler(short opCode, MessageHandler handler)
        {
            if (handler == null)
            {
                _messageHandlers.Remove(opCode);
                return;
            }

            _messageHandlers[opCode] = handler;
        }

        public void RemoveHandler(short opCode, MessageHandler handler = null)
        {
            if (_messageHandlers.ContainsKey(opCode) && (_messageHandlers[opCode] == handler || handler == null))
            {
                _messageHandlers.Remove(opCode);
            }
        }
    }
}