using System.ComponentModel;

namespace RemoteController.Win32.Native
{
    public class MouseHookEventArgs : HandledEventArgs
    {
        public MouseState MouseState { get; }
        public LowLevelMouseInputEvent MouseData { get; }

        public MouseHookEventArgs(
            LowLevelMouseInputEvent mouseData,
            MouseState mouseState)
        {
            MouseData = mouseData;
            MouseState = mouseState;
        }
    }
}