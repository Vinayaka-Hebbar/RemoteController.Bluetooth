using System;

namespace RemoteController.Win32.Hooks
{
    public class ClipboardChangedEventArgs : EventArgs
    {
        public string Value { get; set; }
    }
}
