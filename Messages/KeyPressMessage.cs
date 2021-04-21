using RemoteController.Win32.Hooks;

namespace RemoteController.Messages
{
    public readonly struct KeyPressMessage : IMessage
    {
        public readonly Key Key;
        public readonly bool IsDown;

        public KeyPressMessage(Key key, bool isDown)
        {
            Key = key;
            IsDown = isDown;
        }

        public MessageType Type => MessageType.KeyPress;

        public unsafe byte[] GetBytes()
        {
            var res = new byte[Message.HeaderSize];
            fixed (byte* b = res)
            {
                var bytes = b;
                *bytes = Message.KeyPress;
                // skip the header
                bytes++;
                *(int*)bytes = (int)Key;
                bytes += 4;
                *bytes = (byte)(IsDown ? 1 : 0);
            }
            return res;
        }
    }
}
