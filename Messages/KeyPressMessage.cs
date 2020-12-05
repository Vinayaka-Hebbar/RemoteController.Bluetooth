using RemoteController.Win32.Hooks;
using System.Runtime.CompilerServices;

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

        public unsafe KeyPressMessage(IMessage packet)
        {
            fixed (byte* b = packet.GetBytes())
            {
                Key = (Key)(*(int*)(b + 1));
                IsDown = *(int*)(b + 5) == 1;
            }
        }

        public MessageType Type => MessageType.KeyPress;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte[] GetBytes()
        {
            return GetBytes(Key, IsDown);
        }

        public static unsafe byte[] GetBytes(Key key, bool isDown)
        {
            var res = new byte[Message.HeaderSize];
            fixed (byte* b = res)
            {
                var bytes = b;
                *bytes = Message.KeyPress;
                // skip the header
                bytes++;
                *(int*)bytes = (int)key;
                bytes += 4;
                *bytes = (byte)(isDown ? 1 : 0);
            }
            return res;
        }
    }
}
