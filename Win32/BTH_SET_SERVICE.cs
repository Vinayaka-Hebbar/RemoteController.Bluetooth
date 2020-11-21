using System;
using System.Runtime.InteropServices;

namespace RemoteController.Win32
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct BTH_SET_SERVICE
    {
        public uint pSdpVersion;
        public IntPtr pRecordHandle;
        public ServiceClass fCodService;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 5)]
        uint[] Reserved;
        public uint ulRecordLength;
        public IntPtr pRecord;
    }
}