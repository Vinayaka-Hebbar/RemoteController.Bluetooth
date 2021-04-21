using System;
using System.Net;
using System.Windows.Controls;

namespace RemoteController.Dialogs
{
    /// <summary>
    /// Interaction logic for HostDialog.xaml
    /// </summary>
    public partial class DeviceDialog
    {
        public DeviceDialog()
        {
            InitializeComponent();
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            DeviceTypeCombo.ItemsSource = Enum.GetValues(typeof(Model.RemoteDeviceType));
        }

        public Model.IDeviceOption Device
        {
            get
            {
                if (DeviceContent.Content is Controls.DeviceSelectionBase selection)
                {
                    return selection.GetDeviceOption();
                }
                return null;
            }
        }

        protected override bool Validated()
        {
            if (DeviceContent.Content is Controls.DeviceSelectionBase selection)
                return selection.Validate();
            return false;
        }

        void DeviceTypeSelected(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0 && e.AddedItems[0] is Model.RemoteDeviceType type)
            {
                switch (type)
                {
                    case Model.RemoteDeviceType.Bluetooth:
                        DeviceContent.Content = new Controls.BluetoothSelection();
                        break;
                    case Model.RemoteDeviceType.Socket:
                        DeviceContent.Content = new Controls.SocketSelection();
                        break;
                }
            }
        }
    }
}
