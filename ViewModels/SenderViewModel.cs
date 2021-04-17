using RemoteController.Bluetooth;
using RemoteController.Core;
using RemoteController.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RemoteController.ViewModels
{
    public sealed class SenderViewModel : ViewModelBase
    {
        const string ConnectState = "Connect";
        const string DisconnectState = "Disconnect";

        private static readonly Guid _serviceClassId = new Guid("9bde4762-89a6-418e-bacf-fcd82f1e0677");
        private Device selectedDevice;
        private RemoteClient client;

        public SenderViewModel()
        {
            Devices = new ObservableCollection<Device>()
            {
               new Device(null) { DeviceName = "Searching..." }
            };
        }

        private string state = ConnectState;
        public string State
        {
            get => state;
            set
            {
                state = value;
                OnPropertyChanged(nameof(State));
            }
        }

        private Command.RelayCommand connect;
        public Command.RelayCommand Connect
        {
            get
            {
                if (connect == null)
                    connect = new Command.RelayCommand(OnConnect, d => SelectedDevice != null && SelectedDevice.DeviceInfo != null);
                return connect;
            }
        }

        async void OnConnect(object arg)
        {
            try
            {
                if (client != null)
                {
                    Stop();
                }
                else
                {
                    client = new RemoteClient(new BluetoothEndPoint(SelectedDevice.DeviceInfo.DeviceAddress, _serviceClassId));
                    if (await client.Start())
                    {
                        State = DisconnectState;
                        // client.RunMessageLoop();
                    }
                }
            }
            catch (System.IO.IOException)
            {
                Stop();
            }
        }

        void Stop()
        {
            client.Dispose();
            client = null;
            State = ConnectState;
        }

        public async Task InitAsync()
        {
            var settings = DependencyService.Instance.GetService<IServiceProvider>();
            BluetoothRadio radio = BluetoothRadio.Default;
            if (radio != null && radio.Mode == RadioMode.PowerOff)
            {
                radio.Mode = RadioMode.Connectable;
            }
            IList<Device> devices = await GetDevices();
            Devices.Clear();
            foreach (Device device in devices)
            {
                Devices.Add(device);
            }
        }

        /// <summary>
        /// Gets or sets the devices.
        /// </summary>
        /// <value>
        /// The devices.
        /// </value>
        public ObservableCollection<Device> Devices
        {
            get; 
        }

        public Device SelectedDevice
        {
            get => selectedDevice;
            set
            {
                selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }

        public bool IsConnected => client != null;

        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <returns>The list of the devices.</returns>
        public static async Task<IList<Device>> GetDevices()
        {
            // for not block the UI it will run in a different threat
            Task<List<Device>> task = Task.Run(() =>
            {
                List<Device> devices = new List<Device>();
                IReadOnlyList<BluetoothDeviceInfo> array = BluetoothClient.DiscoverDevices();
                int count = array.Count;
                for (int i = 0; i < count; i++)
                {
                    devices.Add(new Device(array[i]));
                }
                return devices;
            });
            return await task;
        }

    }
}
