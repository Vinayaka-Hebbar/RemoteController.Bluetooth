﻿using RemoteController.Desktop;
using RemoteController.Messages;
using RemoteController.Win32;
using System;
using System.Net;
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
        private bool isDisposed;

        public RemoteClient(EndPoint endPoint)
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
            var displays = _hook.GetDisplays();
            var count = displays.Count;
            var display = displays[0];
            var s = configuration.AddScreen(display, display.GetDpi(), state.ClientName);
            for (int i = 1; i < count; i++)
            {
                display = displays[i];
                s = configuration.AddScreenRight(s, display, display.GetDpi(), state.ClientName);
            }
            _connection.Start();
#if QUEUE_CLIENT
            _dispatcher.StartDispatcher();
#endif
            var localScreens = configuration.AllScreen;
            await _connection.Send(new CheckInMessage(state.ClientName, localScreens).GetBytes());
            var checkIn = await _connection.WaitForCheckIn();
            // server screens
            var screens = checkIn.Screens;
            ScreenConfiguration screenConfiguration = _screen.ScreenConfiguration;
            screenConfiguration.AddScreens(screens);
            screenConfiguration.AddScreensRight(localScreens);
            // Focus Current Client
            state.CurrentClientFocused = true;
            _hook.Hook.SetMousePos(state.LastPositionX, state.LastPositionY);

            _hook.Start();

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

        protected virtual void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    _hook.Dispose();
#if QUEUE_CLIENT
                    _dispatcher.Dispose();
#endif
                    _connection.Send(new CheckOutMessage(state.ClientName));
                    _connection.Dispose();
                }
                isDisposed = true;
            }
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
