namespace RemoteController.Messages
{
    public readonly struct MessageInfo : IMessage
    {
        public readonly int Type;
        public readonly int Length;
        public readonly short PayLoad;

        public unsafe MessageInfo(byte[] buffer)
        {
            fixed (byte* b = buffer)
            {
                Type = *b;
                var res = b;
                res++;
                Length = *(int*)(res);
                res += 4;
                PayLoad = *(short*)res;
            }
        }

        public MessageInfo(int type, int length)
        {
            Type = type;
            Length = length;
            PayLoad = 0;
        }

        public MessageType MessageType => (MessageType)(Type & Message.TypeMask);

        MessageType IMessage.Type => MessageType;

        public unsafe byte[] GetBytes()
        {
            var bytes = new byte[8];
            fixed (byte* b = bytes)
            {
                Message.SetHeader(b, Type, Length);
                var res = b;
                res += 5;
                *(short*)res = PayLoad;
            }
            return bytes;
        }
    }
}