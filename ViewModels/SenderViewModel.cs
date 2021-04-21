using RemoteController.Bluetooth;
using RemoteController.Command;
using RemoteController.Core;
using RemoteController.Model;
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

        private IDeviceOption selectedDevice;
        private RemoteClient client;

        public SenderViewModel()
        {
            Devices = new ObservableCollection<IDeviceOption>();
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

        private ICommand pick;
        public ICommand Pick
        {
            get
            {
                if (pick == null)
                    pick = new RelayCommand(OnPick, (d) => !IsBusy && state.Equals(ConnectState));
                return pick;
            }
        }

        private ICommand connect;
        public ICommand Connect
        {
            get
            {
                if (connect == null)
                    connect = new AsyncRelayCommand(OnConnect, d => selectedDevice != null);
                return connect;
            }
        }

        void OnPick(object _)
        {
            var dialog = new Dialogs.DeviceDialog();
            if (dialog.ShowDialog() == true)
            {
                Devices.Add(dialog.Device);
            }
        }

        async Task OnConnect(object arg)
        {
            try
            {
                if (client != null)
                {
                    Stop();
                }
                else
                {
                    client = new RemoteClient(selectedDevice.GetEndPoint());
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
            try
            {
                client.Dispose();
            }
            finally
            {
                client = null;
                State = ConnectState;
            }
        }

        /// <summary>
        /// Gets or sets the devices.
        /// </summary>
        /// <value>
        /// The devices.
        /// </value>
        public ObservableCollection<IDeviceOption> Devices
        {
            get;
        }

        public IDeviceOption SelectedDevice
        {
            get => selectedDevice;
            set
            {
                selectedDevice = value;
                OnPropertyChanged(nameof(SelectedDevice));
            }
        }

        public bool IsConnected => client != null;
    }
}
