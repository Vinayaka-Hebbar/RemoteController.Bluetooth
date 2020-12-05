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

        public unsafe MouseButtonMessage(IMessage message)
        {
            fixed (byte* b = message.GetBytes())
            {
                var res = b;
                // Skip type and len
                res += 5;
                MouseButton = (MouseButton)(*res);
                IsDown = (*(res + 1)) == 1;
            }
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

        public static unsafe byte[] GetBytes(MouseButton button, bool isDown)
        {
            var res = new byte[8];
            fixed (byte* b = res)
            {
                Message.SetHeader(b, Message.MouseButton, 0);
                var bytes = b;
                // append to header
                bytes += 5;
                *bytes = (byte)button;
                bytes++;
                *bytes = isDown ? (byte)1 : (byte)0;

            }
            return res;
        }
    }
}
