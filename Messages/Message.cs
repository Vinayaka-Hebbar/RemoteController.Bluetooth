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

        public const int TypeOffset = 0xF;

        public unsafe static void SetHeader(byte * b, int type, int len)
        {
            *b = (byte)(type << TypeOffset);
            b++;
            *(int*)b = len;
            b += 4;
        }
    }
}
