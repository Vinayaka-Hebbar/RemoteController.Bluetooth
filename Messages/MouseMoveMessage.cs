namespace RemoteController.Messages
{
    public readonly struct MouseMoveMessage : IMessage
    {
        public readonly double VirtualX;
        public readonly double VirtualY;

        public MouseMoveMessage(double virtualX, double virtualY)
        {
            VirtualX = virtualX;
            VirtualY = virtualY;
        }

        public unsafe MouseMoveMessage(IMessage packet)
        {
            fixed (byte* b = packet.GetBytes())
            {
                VirtualX = *(long*)b;
                VirtualY = *(long*)(b + 8);
            }
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

        public unsafe static byte[] GetBytes(double virtualX, double virtualY)
        {
            var res = new byte[24];
            fixed (byte* b = res)
            {
                Message.SetHeader(b, Message.MouseMove, 16);
                var bytes = b;
                // skip the header bytes
                bytes += 8;
                *(long*)bytes = (long)virtualX;
                bytes += 8;
                *(long*)bytes = (long)virtualY;
            }
            return res;
        }
    }
}
