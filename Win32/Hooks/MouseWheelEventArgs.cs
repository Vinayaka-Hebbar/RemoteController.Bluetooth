namespace RemoteController.Win32.Hooks
{
    public class MouseWheelEventArgs : MouseEventArgs
    {
        public int DeltaX { get; internal set; }
        public int DeltaY { get; set; }
    }
}
