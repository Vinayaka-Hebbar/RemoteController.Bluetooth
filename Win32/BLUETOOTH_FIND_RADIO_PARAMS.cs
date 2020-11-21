using System.Runtime.InteropServices;

namespace RemoteController.Win32
{
    // The BLUETOOTH_FIND_RADIO_PARAMS structure facilitates enumerating installed Bluetooth radios.
    [StructLayout(LayoutKind.Sequential)]
    internal struct BLUETOOTH_FIND_RADIO_PARAMS
    {
        public int dwSize;

        public static BLUETOOTH_FIND_RADIO_PARAMS Create()
        {
            BLUETOOTH_FIND_RADIO_PARAMS p = new BLUETOOTH_FIND_RADIO_PARAMS();
            p.dwSize = Marshal.SizeOf(p);
            return p;
        }
    }
}
