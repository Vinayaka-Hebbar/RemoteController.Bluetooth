using RemoteController.Desktop;
using RemoteController.Messages;
using RemoteController.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
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

            //there is some kind of dpi awareness bug here on windows. not sure exactly what's up.
            foreach (Win32.Hooks.Display display in _hook.GetDisplays())
            {
                state.ScreenConfiguration.AddScreen(display.X, display.Y, display.X, display.Y, display.Width, display.Height, state.ClientName);
            }
            _connection.Start();
            _dispatcher.StartDispatcher();
            await _connection.Send(new CheckInMessage(state.ClientName, state.ScreenConfiguration.Screens.Values.SelectMany(x => x).ToArray()));
            var checkIn = await _connection.WaitForCheckIn();
            var screens = checkIn.Screens;
            ScreenConfiguration screenConfiguration = state.ScreenConfiguration;
            foreach (var screen in screens)
            {
                //Console.WriteLine("Screen:"+screen.X+","+screen.Y + ", LocalX:"+screen.LocalX + ", "+screen.LocalY + " , Width:"+screen.Width + " , height:"+screen.Height+", client: "+ screen.Client);
                if (!screenConfiguration.Screens.ContainsKey(screen.Client))
                {
                    screenConfiguration.Screens.TryAdd(screen.Client, new List<VirtualScreen>());
                    VirtualScreen last = screenConfiguration.GetFurthestRight();
                    screenConfiguration.AddScreenLeft(last, screen.X, screen.Y, screen.Width, screen.Height, screen.Client);
                }

            }
            _hook.Hook.SetMousePos(state.LastPositionX, state.LastPositionY);
            _hook.Start();
            return true;
        }

        public void MoveScreenRight()
        {
            //move the screens for the current client.
            _dispatcher.Process(MoveScreenMessage.Right);
        }

        public void MoveScreenLeft()
        {
            //move the screens for the current client. 
            _dispatcher.Process(MoveScreenMessage.Left);
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
            _dispatcher.Dispose();
        }

    }
}
