using RemoteController.Desktop;
using RemoteController.Win32;
using System;
using System.Linq;

namespace RemoteController.Core
{
    class RemoteClient : IDisposable
    {
        private readonly HookManager _hook;
        private readonly ServerConnectionManager _connection;
        private readonly ServerEventDispatcher _dispatcher;
        private readonly ServerEventReceiver _receiver;
        private readonly VirtualScreenManager _screen;

        private readonly ClientState state;

        public RemoteClient(Bluetooth.BluetoothEndPoint endPoint)
        {
            state = new ClientState(Environment.MachineName);

            _connection = new ServerConnectionManager(endPoint);
            _dispatcher = new ServerEventDispatcher(_connection);
            _screen = new VirtualScreenManager(state);
            _hook = new HookManager(_dispatcher, _screen);
            _receiver = new ServerEventReceiver(_connection, _hook, _screen);

        }

        public bool Start()
        {
            _connection.Start();
            _hook.Init();

            //there is some kind of dpi awareness bug here on windows. not sure exactly what's up.
            VirtualScreen s = null;
            foreach (Win32.Hooks.Display display in _hook.GetDisplays())
            {
                s = state.ScreenConfiguration.AddScreen(display.X, display.Y, display.X, display.Y, display.Width, display.Height, state.ClientName, string.Empty);
            }
            _dispatcher.ClientCheckin(state.ClientName, state.ScreenConfiguration.Screens.Values.SelectMany(x => x).ToList());
            _hook.Hook.SetMousePos(state.LastPositionX, state.LastPositionY);
            _hook.Start();

            return true;
        }

        public void MoveScreenRight()
        {
            //move the screens for the current client.
            _dispatcher.MoveScreenRight();
        }
        public void MoveScreenLeft()
        {
            //move the screens for the current client. 
            _dispatcher.MoveScreenLeft();
        }

        public void RunMessageLoop()
        {
            //Windows needs a message pump
            while (true && NativeMethods.GetMessage(out Msg msg, IntPtr.Zero, 0, 0) > 0)
            {
                NativeMethods.TranslateMessage(ref msg);
                NativeMethods.DispatchMessage(ref msg);
            }
        }

        public void Dispose()
        {
            _hook.Dispose();
            _connection.Dispose();
        }

    }
}
