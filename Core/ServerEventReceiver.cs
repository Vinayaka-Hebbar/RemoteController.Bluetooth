using RemoteController.Desktop;

namespace RemoteController.Core
{
    internal class ServerEventReceiver
    {
        private ServerConnectionManager _connection;
        private HookManager _hook;
        private VirtualScreenManager _screen;

        public ServerEventReceiver(ServerConnectionManager connection, HookManager hook, VirtualScreenManager screen)
        {
            _connection = connection;
            _hook = hook;
            _screen = screen;
        }
    }
}