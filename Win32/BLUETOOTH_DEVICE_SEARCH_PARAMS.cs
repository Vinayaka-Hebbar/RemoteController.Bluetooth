using System;
using System.Runtime.InteropServices;

namespace RemoteController.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BLUETOOTH_DEVICE_SEARCH_PARAMS
    {
        int dwSize;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fReturnAuthenticated;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fReturnRemembered;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fReturnUnknown;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fReturnConnected;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fIssueInquiry;
        public byte cTimeoutMultiplier;
        IntPtr hRadio;

        public static BLUETOOTH_DEVICE_SEARCH_PARAMS Create()
        {
            BLUETOOTH_DEVICE_SEARCH_PARAMS search = new BLUETOOTH_DEVICE_SEARCH_PARAMS();
            search.dwSize = Marshal.SizeOf(search);
            return search;
        }
    }
}