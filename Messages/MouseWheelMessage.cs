using System.Runtime.CompilerServices;

namespace RemoteController.Messages
{
    public readonly struct MouseWheelMessage : IMessage
    {
        public readonly int DeltaX;
        public readonly int DeltaY;

        public MouseWheelMessage(int deltaX, int deltaY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
        }

        public MessageType Type => MessageType.MouseWheel;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte[] GetBytes()
        {
            return GetBytes(DeltaX, DeltaY);
        }

        public static unsafe byte[] GetBytes(int deltaX, int deltaY)
        {
            var res = new byte[Message.HeaderSize];
            fixed (byte* b = res)
            {
                var bytes = b;
                *bytes = Message.MouseWheel;
                // skip the header
                bytes++;
                *(int*)bytes = deltaX;
                bytes += 4;
                *(int*)bytes = deltaY;
            }
            return res;
        }
    }
}
