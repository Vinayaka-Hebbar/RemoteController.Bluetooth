using System.Net.Sockets;

namespace RemoteController.Messages
{
    public struct MouseMoveMessage : IMessage
    {
        public readonly double VirtualX;
        public readonly double VirtualY;

        public MouseMoveMessage(double virtualX, double virtualY)
        {
            VirtualX = virtualX;
            VirtualY = virtualY;
        }

        public MessageType Type => MessageType.MouseMove;

        public unsafe byte[] GetBytes()
        {
            var res = new byte[24];
            fixed (byte* b = res)
            {
                Message.SetHeader(b, Message.MouseMove, 16);
                var bytes = b;
                // skip the header bytes
                bytes += 8;
                *(long*)bytes = (long)VirtualX;
                bytes += 8;
                *(long*)bytes = (long)VirtualY;
            }
            return res;
        }

        public unsafe static MouseMoveMessage Parse(MessageInfo message, NetworkStream stream)
        {
            var buffer = new byte[message.Length];
            if (stream.Read(buffer, 0, message.Length) > 0)
            {
                fixed (byte* b = buffer)
                {
                    var res = b;
                    return new MouseMoveMessage(*(long*)res, *(long*)(res + 8));
                }
            }
            return default;
        }
    }
}
