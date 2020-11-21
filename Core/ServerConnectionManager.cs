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

        public bool IsConnected { get; private set; }

        internal void Start()
        {
            IsConnected = true;
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

        public void Dispose()
        {
            client?.Dispose();
        }
    }
}