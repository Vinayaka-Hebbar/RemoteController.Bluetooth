using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RemoteController.Core
{
    public class ConnectedClient
    {
        private readonly Bluetooth.BluetoothClient client;


        public ConnectedClient(Bluetooth.BluetoothClient client)
        {
            this.client = client;
        }

        public Task InitializeAsync()
        {
            return Task.FromResult(0);
        }
    }
}
