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

        public unsafe byte[] GetBytes()
        {
            var res = new byte[Message.HeaderSize];
            fixed (byte* b = res)
            {
                var bytes = b;
                *bytes = Message.MouseWheel;
                // skip the header
                bytes++;
                *(int*)bytes = DeltaX;
                bytes += 4;
                *(int*)bytes = DeltaY;
            }
            return res;
        }
    }
}
