using RemoteController.Desktop;
using RemoteController.Messages;
using RemoteController.Win32;
using System;
using System.Threading.Tasks;

namespace RemoteController.Core
{
    internal class RemoteClient : IDisposable
    {
        private readonly HookManager _hook;
        private readonly ServerConnectionManager _connection;
        private readonly ServerEventDispatcher _dispatcher;
        private readonly VirtualScreenManager _screen;

        private readonly ClientState state;

        public RemoteClient(Bluetooth.BluetoothEndPoint endPoint)
        {
            state = new ClientState(Environment.MachineName);

            _connection = new ServerConnectionManager(endPoint);
            _dispatcher = new ServerEventDispatcher(_connection);
            _screen = new VirtualScreenManager(state);
            _hook = new HookManager(_dispatcher, _screen);
        }

        public async Task<bool> Start()
        {
            _hook.Init();
            ScreenConfiguration configuration = new ScreenConfiguration();
            //there is some kind of dpi awareness bug here on windows. not sure exactly what's up.
            foreach (Win32.Hooks.Display display in _hook.GetDisplays())
            {
                configuration.AddScreen(display.X, display.Y, display.X, display.Y, display.Width, display.Height, display.GetDpi(), state.ClientName);
            }
            _connection.Start();
#if QUEUE_CLIENT
            _dispatcher.StartDispatcher();
#endif
            await _connection.Send(new CheckInMessage(state.ClientName, configuration.AllScreen).GetBytes());
            var checkIn = await _connection.WaitForCheckIn();
            var screens = checkIn.Screens;
            ScreenConfiguration screenConfiguration = _screen.ScreenConfiguration;
            screenConfiguration.AddScreensRight(screens);
            screenConfiguration.AddScreensLeft(configuration.AllScreen);
            _hook.Start();
            var s = screenConfiguration.GetFurthestLeft();
            state.VirtualX = s.X;
            state.VirtualY = s.Y;
            if (s.Client == state.ClientName)
            {
                _hook.Hook.SetMousePos(state.LastPositionX, state.LastPositionY);
                state.CurrentClientFocused = true;
            }
            return true;
        }

        public void MoveScreenRight()
        {
            //move the screens for the current client.
#if QUEUE_CLIENT
            _dispatcher.Process(MoveScreenMessage.Right);
#else
            _dispatcher.Send(MoveScreenMessage.Right.GetBytes());
#endif
        }

        public void MoveScreenLeft()
        {
            //move the screens for the current client. 
#if QUEUE_CLIENT
            _dispatcher.Process(MoveScreenMessage.Left);
#else
            _dispatcher.Send(MoveScreenMessage.Left.GetBytes());
#endif
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
#if QUEUE_CLIENT
            _dispatcher.Dispose();
#endif
        }

    }
}
