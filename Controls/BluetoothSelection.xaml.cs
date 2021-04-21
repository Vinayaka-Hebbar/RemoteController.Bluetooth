using RemoteController.Bluetooth;
using RemoteController.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace RemoteController.Controls
{
    /// <summary>
    /// Interaction logic for BluetoothSelection.xaml
    /// </summary>
    public partial class BluetoothSelection
    {
        public BluetoothSelection()
        {
            InitializeComponent();
        }

        public override IDeviceOption GetDeviceOption()
        {
            if (DeviceList.SelectedItem is Device device)
            {
                return new BluetoothOption(device.DeviceInfo);
            }
            return null;
        }

        public override bool Validate()
        {
            return DeviceList.SelectedItem is Device device && device.DeviceInfo != null;
        }

        protected async override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            BluetoothRadio radio = BluetoothRadio.Default;
            if (radio != null && radio.Mode == RadioMode.PowerOff)
            {
                radio.Mode = RadioMode.Connectable;
            }
            var devices = new ObservableCollection<Device>()
            {
                new Device(null){DeviceName = "Searching..."}
            };
            DeviceList.ItemsSource = devices;
            var res = await GetDevices();
            devices.Clear();
            foreach (var device in res)
            {
                devices.Add(device);
            }
        }

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
