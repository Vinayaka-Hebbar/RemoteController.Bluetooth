using RemoteController.Win32.Hooks;

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

        public MessageType Type => MessageType.MouseButton;

        public unsafe byte[] GetBytes()
        {
            var res = new byte[Message.HeaderSize];
            fixed (byte* b = res)
            {
                var bytes = b;
                *bytes = Message.MouseButton;
                // append to header
                bytes++;
                *bytes = (byte)MouseButton;
                bytes++;
                *bytes = IsDown ? (byte)1 : (byte)0;

            }
            return res;
        }
    }
}
