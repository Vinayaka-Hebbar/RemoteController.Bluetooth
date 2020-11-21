using RemoteController.Bluetooth;
using RemoteController.Model;
using RemoteController.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace RemoteController.ViewModels
{
    public sealed class SenderViewModel : ViewModelBase
    {
        const string ConnectState = "Connect";
        const string DisconnectState = "Disconnect";

        private readonly Guid _serviceClassId;
        private Device selectedDevice;
        private SenderService task;

        public SenderViewModel()
        {
            _serviceClassId = new Guid("9bde4762-89a6-418e-bacf-fcd82f1e0677");
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

        private Command.RelayCommand textChanged;
        public Command.RelayCommand TextChanged
        {
            get
            {
                if (textChanged == null)
                    textChanged = new Command.RelayCommand(OnTextInput);
                return textChanged;
            }
        }

        private string textIn;
        public string TextIn
        {
            get => textIn;
            set
            {
                textIn = value;
                OnPropertyChanged(nameof(TextIn));
            }
        }

        private void OnTextInput(object arg)
        {
            if (arg is KeyEventArgs e)
            {
                char keyCode = (char)KeyInterop.VirtualKeyFromKey(e.Key);
                if (Console.CapsLock == false)
                    keyCode = char.ToLower(keyCode);
                TextIn = keyCode.ToString();
                e.Handled = true;
            }
        }

        private void OnConnect(object arg)
        {
            task = new SenderService(SelectedDevice, _serviceClassId);
            task.Completed += OnStop;
            ServiceContainer.Current.AddSevice(task);
            task.Start();
            State = DisconnectState;
        }

        private void OnStop(TaskService obj)
        {
            ServiceContainer.Current.RemoveService(obj.Id);
            State = ConnectState;
        }

        public async Task InitAsync()
        {
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
            task = ServiceContainer.Current.GetService<SenderService>(_serviceClassId);
            if (task != null)
            {
                Device device = task.Device;
                if (Devices.Contains(device))
                {
                    SelectedDevice = device;
                    State = DisconnectState;
                }
                else
                {
                    task.Dispose();
                }
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
            get; set;
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

        BluetoothClient bluetoothClient;
        public BluetoothClient Client
        {
            get
            {
                if (bluetoothClient == null)
                {
                    System.Threading.Interlocked.CompareExchange(ref bluetoothClient, new BluetoothClient(), null);
                }
                return bluetoothClient;
            }
        }

        /// <summary>
        /// Gets the devices.
        /// </summary>
        /// <returns>The list of the devices.</returns>
        public async Task<IList<Device>> GetDevices()
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
