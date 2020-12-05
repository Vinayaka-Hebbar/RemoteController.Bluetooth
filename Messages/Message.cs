namespace RemoteController.Messages
{
    public static class Message
    {
        public const int MoveScreen = (int)MessageType.MoveScreen,
                         MouseWheel = (int)MessageType.MouseWheel,
                         MouseButton = (int)MessageType.MouseButton,
                         MouseMove = (int)MessageType.MouseMove,
                         KeyPress = (int)MessageType.KeyPress,
                         Clipboard = (int)MessageType.Clipboard,
                         CheckIn = (int)MessageType.CheckIn;

        public const int TypeMask = 0xF;

        public const int HeaderSize = 10;

        public unsafe static void SetHeader(byte* b, int type, int len)
        {
            *b = (byte)type;
            b++;
            *(int*)b = len;
        }
    }
}
