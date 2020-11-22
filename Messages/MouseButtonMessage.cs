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
            var res = new byte[8];
            fixed (byte* b = res)
            {
                Message.SetHeader(b, Message.MouseButton, 0);
                var bytes = b;
                // append to header
                bytes += 5;
                *bytes = (byte)MouseButton;
                bytes++;
                *bytes = IsDown ? (byte)1 : (byte)0;

            }
            return res;
        }

        public unsafe static MouseButtonMessage Parse(IMessage message)
        {
            var bytes = message.GetBytes();
            fixed (byte* b = bytes)
            {
                var res = b;
                // Skip type and len
                res += 5;
                return new MouseButtonMessage((MouseButton)(*res), (*(res + 1)) == 1);
            }
        }
    }
}
