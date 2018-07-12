using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameCloud.Core
{
    /// <summary>
    /// Game cloud client
    /// </summary>
    public class GcConnection
    {
        public delegate void DisconnectHandler(GcConnection connection);

        public static int EstablishPeerTimeoutMillis = 10000;
        public static int RequestTimeoutMillis = 30 * 1000;

        /// <summary>
        /// For generating instance ids. Should be above 0, because 0 will be 
        /// returned in concurrent dictionaries when there's no value
        /// </summary>
        private static int _instanceIdGenerator = 1;

        private readonly int _instanceId;
        private int _requestId;

        public IConnectionImplementation Implementation { get; }

        private NetWriter _writer;

        private object _establishPeerLock = new object();
        private TaskCompletionSource<bool> _establishPeerSource;

        private ConcurrentDictionary<int, TaskCompletionSource<GcMessage>> _responseCallbacks;

        private GcMessage _timeoutMessage;
        public event DisconnectHandler Disconnected; 

        public GcConnection(IConnectionImplementation implementation)
        {
            // Generate instance id for better hashing
            _instanceId = Interlocked.Increment(ref _instanceIdGenerator);
            _responseCallbacks = new ConcurrentDictionary<int, TaskCompletionSource<GcMessage>>();
            _timeoutMessage = new GcMessage(null, 0)
            {
                Status = ResponseStatus.Timeout
            };

            Implementation = implementation;

            implementation.DataReceived += OnDataReceived;
            implementation.Disconnected += OnDisconnected;

            _writer = new NetWriter();
        }

        private void OnDisconnected()
        {
            Disconnected?.Invoke(this);
        }

        private void OnDataReceived(byte[] data)
        {
            if (data.Length == 0)
                return;

            var flags = data[0];

            if ((flags & MessageFlags.InternalMessage) > 0)
            {
                HandleInternalMessage(data);
                return;
            }

            var msg = GcProtocol.ParseMessage(null, data);

            // TODO Try catch

            if (msg.ResponseId >= 0)
            {
                // In case it's a response to a message
                TaskCompletionSource<GcMessage> source;
                _responseCallbacks.TryRemove(msg.ResponseId, out source);

                source?.TrySetResult(msg);
                return;
            }

            // TODO Handle a regular message
        }

        private void HandleInternalMessage(byte[] data)
        {
            // Received confirmation of peer establishment
            if (data.Length >= 3 && data[1] == (byte) InternalOpCode.EstablishPeer)
            {
                var stats = data[2];
                lock (_establishPeerLock)
                {
                    _establishPeerSource?.TrySetResult(stats == 1);
                }
            }
        }

        public void Send(short opCode, Action<NetWriter> writeAction)
        {
            SendBasicMessage(opCode, writeAction, null, null, null, null);
        }

        public async Task<GcMessage> SendRequest(short opCode, Action<NetWriter> writeAction)
        {
            var requestId = Interlocked.Increment(ref _requestId);
            var completionSource = new TaskCompletionSource<GcMessage>();
            _responseCallbacks.TryAdd(requestId, completionSource);

            StartTimeout(completionSource, requestId, RequestTimeoutMillis);

            // Send the message
            SendBasicMessage(opCode, writeAction, requestId, null, null, 0);

            return await completionSource.Task;
        }

        private void SendBasicMessage(short opCode, Action<NetWriter> writeAction,
            int? requestId, int? responseId, ResponseStatus? status, int? peerId, byte defaultFlags = 0)
        {
            byte[] data;

            lock (_writer)
            {
                GcProtocol.PackMessage(_writer, opCode, writeAction, requestId, responseId, status, peerId, defaultFlags);
                data = _writer.ToArray();
            }

            Implementation.SendRawData(data);
        }

        public async Task StartTimeout(TaskCompletionSource<GcMessage> source, int requestId, int timeoutMillis)
        {
            await Task.WhenAny(source.Task, Task.Delay(timeoutMillis));

            if (source.Task.IsCompleted)
                return;

            TaskCompletionSource<GcMessage> removedSource;
            if (_responseCallbacks.TryRemove(requestId, out removedSource))
            {
                removedSource.TrySetResult(_timeoutMessage);
            }
        }

        public async Task<bool> EstablishPeer(TimeSpan timeout)
        {
            TaskCompletionSource<bool> source;
            var sendRequest = false;

            lock (_establishPeerLock)
            {
                source = _establishPeerSource;

                if (source == null)
                {
                    // Source is null - thre are no pending requests
                    source = new TaskCompletionSource<bool>();
                    _establishPeerSource = source;
                    sendRequest = true;
                }
            }

            if (sendRequest)
            {
                // Send a request to establish peer
                Implementation.SendRawData(new [] { MessageFlags.InternalMessage, (byte)InternalOpCode.EstablishPeer });
            }
            
            // Wait for confirmation or timeout
            await Task.WhenAny(source.Task, Task.Delay(EstablishPeerTimeoutMillis));

            if (source.Task.IsCompleted)
                return source.Task.Result;

            return false;
        }

        public Task<bool> ConnectTo(string host, int port)
        {
            return Implementation.Connect(host, port);
        }

        public Task<bool> ConnectTo(string host, int port, out string error)
        {
            return Implementation.Connect(host, port, out error);
        }

        public override int GetHashCode()
        {
            return _instanceId;
        }

        public void Disconnect()
        {
            Implementation.Disconnect();
        }
    }
}