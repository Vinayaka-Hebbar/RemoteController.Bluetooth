namespace RemoteController.Messages
{
    public readonly struct MoveScreenMessage : IMessage
    {
        public static readonly MoveScreenMessage Empty = new MoveScreenMessage(ScreenLocation.None);

        public static readonly MoveScreenMessage Left = new MoveScreenMessage(ScreenLocation.Left);

        public static readonly MoveScreenMessage Right = new MoveScreenMessage(ScreenLocation.Right);

        public const int Offset = 4;

        public readonly ScreenLocation Location;

        public MoveScreenMessage(ScreenLocation location)
        {
            Location = location;
        }

        public MessageType Type => MessageType.MoveScreen;

        public byte[] GetBytes()
        {
            var res = new byte[8];
            res[1] = (byte)((byte)MessageType.MoveScreen | ((int)Location << Offset));
            return res;
        }

        public static MoveScreenMessage Parse(IMessage message)
        {
            var bytes = message.GetBytes();
            if (bytes.Length > 0)
            {
                var header = bytes[1];
                return new MoveScreenMessage((ScreenLocation)(header >> Offset));
            }
            return Empty;
        }
    }

    public enum ScreenLocation : byte
    {
        None,
        Left,
        Right
    }
}
