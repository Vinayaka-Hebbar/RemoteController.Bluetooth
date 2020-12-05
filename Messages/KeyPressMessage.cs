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
                Key = (Key)(*(int*)b);
                IsDown = *(int*)(b + 4) == 1;
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
            var res = new byte[13];
            fixed (byte* b = res)
            {
                Message.SetHeader(b, Message.KeyPress, 5);
                var bytes = b;
                // skip the header
                bytes += 8;
                *(int*)bytes = (int)key;
                bytes += 4;
                *bytes = (byte)(isDown ? 1 : 0);
            }
            return res;
        }
    }
}
