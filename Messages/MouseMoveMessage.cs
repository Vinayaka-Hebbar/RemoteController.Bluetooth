using System.Runtime.CompilerServices;

namespace RemoteController.Messages
{
    public readonly struct MouseMoveMessage : IMessage
    {
        public readonly int VirtualX;
        public readonly int VirtualY;

        public MouseMoveMessage(int virtualX, int virtualY)
        {
            VirtualX = virtualX;
            VirtualY = virtualY;
        }

        public unsafe MouseMoveMessage(IMessage packet)
        {
            fixed (byte* b = packet.GetBytes())
            {
                VirtualX = *(int*)(b + 1);
                VirtualY = *(int*)(b + 5);
            }
        }

        public MessageType Type => MessageType.MouseMove;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte[] GetBytes()
        {
            return GetBytes(VirtualX, VirtualY);
        }

        public unsafe static byte[] GetBytes(int virtualX, int virtualY)
        {
            var res = new byte[Message.HeaderSize];
            fixed (byte* b = res)
            {
                var bytes = b;
                *bytes = Message.MouseMove;
                // skip the header bytes
                bytes++;
                *(int*)bytes = virtualX;
                bytes += 4;
                *(int*)bytes = virtualY;
            }
            return res;
        }
    }
}
