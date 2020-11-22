namespace RemoteController.Messages
{
    public readonly struct MessageInfo : IMessage
    {
        public readonly int Type;
        public readonly int Length;

        public unsafe MessageInfo(byte[] buffer)
        {
            fixed (byte* b = buffer)
            {
                Type = *b;
                Length = *(int*)(b + 1);
            }
        }

        public MessageInfo(int type, int length)
        {
            Type = type;
            Length = length;
        }

        public MessageType MessageType => (MessageType)(Type & Message.TypeMask);

        MessageType IMessage.Type => MessageType;

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