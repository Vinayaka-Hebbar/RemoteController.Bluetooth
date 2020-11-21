using RemoteController.Bluetooth;
using RemoteController.Win32;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RemoteController
{
    public sealed partial class BluetoothDevicePicker
    {
        public Task<BluetoothDeviceInfo> PickSingleDeviceAsync()
        {
            BluetoothDeviceInfo info = null;

            BLUETOOTH_SELECT_DEVICE_PARAMS p = new BLUETOOTH_SELECT_DEVICE_PARAMS();
            p.Reset();
            p.SetClassOfDevices(ClassOfDevices.ToArray());
            p.fForceAuthentication = RequireAuthentication;
            p.hwndParent = NativeMethods.GetActiveWindow();
            if (NativeMethods.BluetoothSelectDevices(ref p))
            {
                info = new BluetoothDeviceInfo(p.Device);
            }

            return Task.FromResult(info);
        }

        public List<ClassOfDevice> ClassOfDevices { get; } = new List<ClassOfDevice>();

        public bool RequireAuthentication { get; set; }
    }
}
