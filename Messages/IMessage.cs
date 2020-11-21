namespace RemoteController.Messages
{
    public interface IMessage
    {
        MessageType Type { get; }
        byte[] GetBytes();
    }
}
