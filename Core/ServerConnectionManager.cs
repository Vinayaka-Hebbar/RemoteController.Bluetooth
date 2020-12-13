using RemoteController.Bluetooth;
using RemoteController.Messages;
using System;
using System.Threading.Tasks;

namespace RemoteController.Core
{
    public class ServerConnectionManager : IDisposable
    {
        private readonly BluetoothEndPoint endPoint;
        private BluetoothClient client;
        private System.IO.Stream stream;

        public bool IsConnected => client != null && client.Connected;

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

        public void Send(IMessage message)
        {
            if (stream != null)
            {
                var bytes = message.GetBytes();
                stream.Write(bytes, 0, bytes.Length);
                stream.Flush();
            }
        }

        public async Task Send(byte[] message)
        {
            if (stream != null)
            {
                await stream.WriteAsync(message, 0, message.Length, cancellationToken: System.Threading.CancellationToken.None);
                await stream.FlushAsync(System.Threading.CancellationToken.None);
            }
        }

        public async Task<CheckInMessage> WaitForCheckIn()
        {
            if (stream != null)
            {
                var bytes = new byte[Message.HeaderSize];
                if (await stream.ReadAsync(bytes, 0, Message.HeaderSize) > 0)
                {
                    var message = new MessageInfo(bytes);
                    if (message.Type == MessageType.CheckIn)
                    {
                        return new CheckInMessage(MessagePacket.Parse(message, stream));
                    }
                }
            }
            return default;
        }

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}