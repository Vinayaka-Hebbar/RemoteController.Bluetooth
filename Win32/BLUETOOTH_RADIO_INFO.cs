using System.Runtime.InteropServices;

namespace RemoteController.Win32
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    internal struct BLUETOOTH_RADIO_INFO
    {
        private const int BLUETOOTH_MAX_NAME_SIZE = 248;

        internal int dwSize;
        internal ulong address;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = BLUETOOTH_MAX_NAME_SIZE)]
        internal string szName;
        internal uint ulClassofDevice;
        internal ushort lmpSubversion;
        [MarshalAs(UnmanagedType.U2)]
        internal ushort manufacturer;

        public static BLUETOOTH_RADIO_INFO Create()
        {
            BLUETOOTH_RADIO_INFO r = new BLUETOOTH_RADIO_INFO();
            r.dwSize = Marshal.SizeOf(r);
            return r;
        }
    }
}