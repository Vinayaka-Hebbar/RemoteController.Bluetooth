namespace RemoteController.Messages
{
    public enum MessageType : byte
    {
        MoveScreen = 1,
        MouseWheel = 2,
        MouseButton = 3,
        MouseMove = 4,
        KeyPress = 5,
        Clipboard = 6,
        CheckIn = 7,
        CheckOut = 8
    }
}
