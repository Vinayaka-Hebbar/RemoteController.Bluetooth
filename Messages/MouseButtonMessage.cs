using RemoteController.Win32.Hooks;
using System.Runtime.CompilerServices;

namespace RemoteController.Messages
{
    public readonly struct MouseButtonMessage : IMessage
    {
        public readonly MouseButton MouseButton;

        public readonly bool IsDown;

        public MouseButtonMessage(MouseButton mouseButton, bool isDown)
        {
            MouseButton = mouseButton;
            IsDown = isDown;
        }

        public unsafe MouseButtonMessage(IMessage message)
        {
            fixed (byte* b = message.GetBytes())
            {
                MouseButton = (MouseButton)(*(b + 1));
                IsDown = (*(b + 2)) == 1;
            }
        }

        public MessageType Type => MessageType.MouseButton;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte[] GetBytes()
        {
            return GetBytes(MouseButton, IsDown);
        }

        public static unsafe byte[] GetBytes(MouseButton button, bool isDown)
        {
            var res = new byte[Message.HeaderSize];
            fixed (byte* b = res)
            {
                var bytes = b;
                *bytes = Message.MouseButton;
                // append to header
                bytes++;
                *bytes = (byte)button;
                bytes++;
                *bytes = isDown ? (byte)1 : (byte)0;

            }
            return res;
        }
    }
}
