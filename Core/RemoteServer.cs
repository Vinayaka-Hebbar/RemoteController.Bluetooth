using RemoteController.Desktop;
using System;
using System.Net;

namespace RemoteController.Core
{
    public class RemoteServer : IDisposable
    {
        private readonly ServerEventReceiver _receiver;
        private readonly VirtualScreenManager _screen;
        private readonly ClientState state;

        public VirtualScreenManager Screens => _screen;

        public RemoteServer(Model.IDeviceOption option)
        {
            state = new ClientState(Environment.MachineName);
            _screen = new VirtualScreenManager(state);
            _receiver = new ServerEventReceiver(_screen, option);
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
