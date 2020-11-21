using RemoteController.Win32.Desktop;
using System;

namespace RemoteController.Win32
{
    public struct Msg
    {
        public IntPtr hwnd;
        public uint message;
        public IntPtr wParam;
        public IntPtr lParam;
        public uint time;
        public Point pt;
    }
}