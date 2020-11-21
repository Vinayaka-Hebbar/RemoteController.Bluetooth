using System.ComponentModel;

namespace RemoteController.Win32.Native
{
    public class MouseHookEventArgs : HandledEventArgs
    {
        public MouseState MouseState { get; private set; }
        public LowLevelMouseInputEvent MouseData { get; private set; }

        public MouseHookEventArgs(
            LowLevelMouseInputEvent mouseData,
            MouseState mouseState)
        {
            MouseData = mouseData;
            MouseState = mouseState;
        }
    }
}