namespace RemoteController.Sockets
{
    public interface ISocketListener
    {
        void Start();
        void Start(int backlog);
        ISocketClient AcceptClient();
        void Stop();
    }
}