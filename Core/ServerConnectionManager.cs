using RemoteController.Bluetooth;
using RemoteController.Messages;
using System;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RemoteController.Core
{
    public class ServerConnectionManager : IDisposable
    {
        private readonly BluetoothEndPoint endPoint;
        private BluetoothClient client;
        private NetworkStream stream;


        public ServerConnectionManager(BluetoothEndPoint endPoint)
        {
            this.endPoint = endPoint;
        }

        internal void Start()
        {
            client = new BluetoothClient();
            client.Connect(endPoint);
            stream = client.GetStream();
        }

        public async Task Send(IMessage message)
        {
            if (stream != null)
            {
                var bytes = message.GetBytes();
                await stream.WriteAsync(bytes, 0, bytes.Length);
                await stream.FlushAsync();
            }
        }

        public Task<CheckInMessage> WaitForCheckIn()
        {
            if (stream != null)
            {
                var bytes = new byte[8];
                if(stream.Read(bytes, 0 , 8) > 0)
                {
                    var message = new MessageInfo(bytes);
                    if(message.MessageType == MessageType.CheckIn)
                    {
                        return Task.FromResult(CheckInMessage.Parse(message, stream));
                    }
                }
            }
            return Task.FromResult(default(CheckInMessage));
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}