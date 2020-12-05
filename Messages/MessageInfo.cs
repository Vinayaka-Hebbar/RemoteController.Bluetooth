namespace RemoteController.Messages
{
    public readonly struct MessageInfo : IMessage
    {
        public readonly byte[] Header;

        public int Length
        {
            get
            {
                unsafe
                {
                    fixed (byte* b = Header)
                    {
                        return *(int*)(b + 1);
                    }
                }
            }
        }

        public MessageInfo(byte[] buffer)
        {
            Header = buffer;
        }

        public MessageType Type => (MessageType)(Header[0] & Message.TypeMask);

        public byte[] GetBytes()
        {
            return Header;
        }
    }
}