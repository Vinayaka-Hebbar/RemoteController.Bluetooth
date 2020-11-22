using RemoteController.Desktop;
using System;

namespace RemoteController.Core
{
    public class RemoteServer : IDisposable
    {
        private readonly ServerEventReceiver _receiver;
        private readonly VirtualScreenManager _screen;
        private readonly ClientState state;

        public RemoteServer()
        {
            state = new ClientState(Environment.MachineName);
            _screen = new VirtualScreenManager(state);
            _receiver = new ServerEventReceiver(_screen);
        }

        public bool Start()
        {
            _receiver.Start();
            return true;
        }

        public void Dispose()
        {
            _receiver.Dispose();
        }
    }
}
