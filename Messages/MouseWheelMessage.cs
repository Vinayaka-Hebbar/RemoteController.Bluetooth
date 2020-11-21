using System.Net.Sockets;

namespace RemoteController.Messages
{
    public struct MouseWheelMessage : IMessage
    {
        public readonly int DeltaX;
        public readonly int DeltaY;

        public MouseWheelMessage(int deltaX, int deltaY)
        {
            DeltaX = deltaX;
            DeltaY = deltaY;
        }

        public MessageType Type => MessageType.MouseWheel;

        public unsafe byte[] GetBytes()
        {
            var res = new byte[16];
            fixed(byte *b = res)
            {
                Message.SetHeader(b, Message.MouseWheel, 8);
                var bytes = b;
                // skip the header
                bytes += 3;
                *(int*)bytes = DeltaX;
                bytes += 4;
                *(int*)bytes = DeltaY;
            }
            return res;
        }

        public static unsafe MouseWheelMessage Parse(MessageInfo info, NetworkStream stream)
        {
            var buffer = new byte[info.Length];
            if(stream.Read(buffer, 0, info.Length) > 0)
            {
                fixed(byte * b = buffer)
                {
                    return new MouseWheelMessage(*(int*)b, *(int*)(b + 4));
                }
            }
            return default;
        }
    }
}
