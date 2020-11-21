using RemoteController.Win32;
using System;
using System.Collections.Generic;

namespace RemoteController.Bluetooth
{
    public sealed class BluetoothDeviceInfo
    {
        private BLUETOOTH_DEVICE_INFO _info;

        internal BluetoothDeviceInfo(BLUETOOTH_DEVICE_INFO info)
        {
            _info = info;
        }

        public BluetoothDeviceInfo(BluetoothAddress address)
        {
            _info = BLUETOOTH_DEVICE_INFO.Create();
            _info.Address = address;
            NativeMethods.BluetoothGetDeviceInfo(IntPtr.Zero, ref _info);
        }

        public void Refresh()
        {
            NativeMethods.BluetoothGetDeviceInfo(IntPtr.Zero, ref _info);
        }

        public BluetoothAddress DeviceAddress
        {
            get
            {
                return _info.Address;
            }
        }

        public string DeviceName
        {
            get
            {
                return _info.szName.TrimEnd();
            }
        }

        #region Last Seen
        public DateTime LastSeen
        {
            get
            {
                NativeMethods.BluetoothGetDeviceInfo(IntPtr.Zero, ref _info);

                return _info.LastSeen;
            }
        }

        public void SetDiscoveryTime(DateTime dt)
        {
            if (LastSeen != DateTime.MinValue)
                throw new InvalidOperationException("LastSeen is already set.");
            _info.stLastSeen = SYSTEMTIME.FromDateTime(dt);
        }
        #endregion

        #region Last Used
        public DateTime LastUsed
        {
            get
            {
                NativeMethods.BluetoothGetDeviceInfo(IntPtr.Zero, ref _info);
                return _info.LastUsed;
            }
        }
        #endregion

        public ClassOfDevice ClassOfDevice
        {
            get
            {
                return (ClassOfDevice)_info.ulClassofDevice;
            }
        }

        public IReadOnlyCollection<Guid> InstalledServices
        {
            get
            {
                int serviceCount = 0;
                _ = NativeMethods.BluetoothEnumerateInstalledServices(IntPtr.Zero, ref _info, ref serviceCount, null);
                byte[] services = new byte[serviceCount * 16];
                int result = NativeMethods.BluetoothEnumerateInstalledServices(IntPtr.Zero, ref _info, ref serviceCount, services);
                if (result < 0)
                    return new Guid[0];

                List<Guid> foundServices = new List<Guid>();
                byte[] buffer = new byte[16];

                for (int s = 0; s < serviceCount; s++)
                {
                    Buffer.BlockCopy(services, s * 16, buffer, 0, 16);
                    foundServices.Add(new Guid(buffer));
                }

                return foundServices.AsReadOnly();
            }
        }

        public bool Connected
        {
            get
            {
                return _info.fConnected;
            }
        }

        public bool Authenticated
        {
            get
            {
                return _info.fAuthenticated;
            }
        }

        #region Remembered
        /// <summary>
        /// Specifies whether the device is a remembered device. Not all remembered devices are authenticated.
        /// </summary>
        /// -
        /// <remarks>Now supported under Windows CE &#x2014; will return the same as 
        /// <see cref="BluetoothDeviceInfo.Authenticated"/>.
        /// </remarks>
        /// <seealso cref="Connected"/>
        /// <seealso cref="Authenticated"/>
        public bool Remembered
        {
            get
            {
                NativeMethods.BluetoothGetDeviceInfo(IntPtr.Zero, ref _info);
                return _info.fRemembered;
            }
        }
        #endregion

        public void SetServiceState(Guid service, bool state)
        {
            _ = NativeMethods.BluetoothSetServiceState(IntPtr.Zero, ref _info, ref service, state ? 1u : 0);
        }

        public bool Equals(BluetoothDeviceInfo other)
        {
            if (other is null)
                return false;

            return DeviceAddress == other.DeviceAddress;
        }
    }
}
