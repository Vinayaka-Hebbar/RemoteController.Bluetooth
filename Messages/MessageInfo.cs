namespace RemoteController.Messages
{
    public struct MessageInfo : IMessage
    {
        public readonly int Type;
        public readonly int Length;

        public unsafe MessageInfo(byte[] buffer)
        {
            fixed (byte* b = buffer)
            {
                Type = *b;
                Length = *(int*)(b + 4);
            }
        }

        public MessageInfo(int type, int length)
        {
            this.Type = type;
            Length = length;
        }

        MessageType IMessage.Type => (MessageType)(Type & Message.TypeMask);

        public unsafe byte[] GetBytes()
        {
            var bytes = new byte[8];
            fixed (byte* b = bytes)
            {
                Message.SetHeader(b, Type, Length);
            }
            return bytes;
        }
    }
}