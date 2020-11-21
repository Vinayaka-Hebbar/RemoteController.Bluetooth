using RemoteController.Win32.Native;
using System.ComponentModel;

namespace RemoteController.Win32.Hooks
{
    public class KeyboardHookEventArgs : HandledEventArgs
    {
        public Native.KeyboardState KeyboardState { get; private set; }
        public LowLevelKeyboardInputEvent KeyboardData { get; private set; }

        public KeyboardHookEventArgs(
            LowLevelKeyboardInputEvent keyboardData,
            Native.KeyboardState keyboardState)
        {
            KeyboardData = keyboardData;
            KeyboardState = keyboardState;
        }
    }
}