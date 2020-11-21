using RemoteController.Win32;
using System;

namespace RemoteController.Bluetooth
{
    public sealed class BluetoothRadio : IDisposable
    {
        private static BluetoothRadio s_default;

        public static BluetoothRadio Default
        {
            get
            {
                if (s_default == null)
                {
                    s_default = GetDefault();
                }

                return s_default;
            }
        }

        public string Name
        {
            get
            {
                return _radio.szName;
            }
        }

        public BluetoothAddress LocalAddress
        {
            get
            {
                return _radio.address;
            }
        }

        public RadioMode Mode
        {
            get
            {
                if (NativeMethods.BluetoothIsDiscoverable(_handle))
                {
                    return RadioMode.Discoverable;
                }

                if (NativeMethods.BluetoothIsConnectable(_handle))
                {
                    return RadioMode.Connectable;
                }

                return RadioMode.PowerOff;
            }
            set
            {
                switch (value)
                {
                    case RadioMode.Discoverable:
                        if (Mode == RadioMode.PowerOff)
                        {
                            NativeMethods.BluetoothEnableIncomingConnections(_handle, true);
                        }

                        NativeMethods.BluetoothEnableDiscovery(_handle, true);
                        break;

                    case RadioMode.Connectable:
                        if (Mode == RadioMode.Discoverable)
                        {
                            NativeMethods.BluetoothEnableDiscovery(_handle, false);
                        }
                        else
                        {
                            NativeMethods.BluetoothEnableIncomingConnections(_handle, true);
                        }
                        break;

                    case RadioMode.PowerOff:
                        if (Mode == RadioMode.Discoverable)
                        {
                            NativeMethods.BluetoothEnableDiscovery(_handle, false);
                        }

                        NativeMethods.BluetoothEnableIncomingConnections(_handle, false);
                        break;
                }
            }
        }

        ~BluetoothRadio()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private static BluetoothRadio GetDefault()
        {
            BLUETOOTH_FIND_RADIO_PARAMS p = BLUETOOTH_FIND_RADIO_PARAMS.Create();
            BLUETOOTH_RADIO_INFO info = BLUETOOTH_RADIO_INFO.Create();
            IntPtr findHandle = NativeMethods.BluetoothFindFirstRadio(ref p, out IntPtr hRadio);

            if (hRadio != IntPtr.Zero)
            {
                _ = NativeMethods.BluetoothGetRadioInfo(hRadio, ref info);
            }

            if (findHandle != IntPtr.Zero)
            {
                NativeMethods.BluetoothFindRadioClose(findHandle);
            }

            if (hRadio != IntPtr.Zero)
            {
                return new BluetoothRadio(info, hRadio);
            }

            return null;
        }

        private readonly BLUETOOTH_RADIO_INFO _radio;
        private IntPtr _handle;

        private BluetoothRadio(BLUETOOTH_RADIO_INFO info, IntPtr handle)
        {
            _radio = info;
            _handle = handle;
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                }

                if (_handle != IntPtr.Zero)
                {
                    NativeMethods.CloseHandle(_handle);
                    _handle = IntPtr.Zero;
                }

                disposedValue = true;
            }
        }

        #endregion
    }

    public enum RadioMode
    {
        PowerOff,
        Connectable,
        Discoverable,
    }
}
