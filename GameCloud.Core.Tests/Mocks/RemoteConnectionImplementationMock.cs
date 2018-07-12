using System;

namespace GameCloud.Core.Tests.Mocks
{
    public class RemoteConnectionImplementationMock: IRemoteConnectionImplementation
    {
        private ConnectionMock _connection;

        public void SetConnectionMock(ConnectionMock connection)
        {
            _connection = connection;
        }
        
        public void SendRawData(byte[] data)
        {
            _connection.HandleRawData(data);
        }
    }
}