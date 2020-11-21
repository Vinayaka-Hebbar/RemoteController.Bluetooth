using RemoteController.Win32.Desktop;
using System;
using System.Runtime.InteropServices;

namespace RemoteController.Win32.Native
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LowLevelMouseInputEvent
    {
        public Point Point;

        public uint Mousedata;

        public uint Flags;

        public uint Time;

        public IntPtr AdditionalInformation;

    }
}
