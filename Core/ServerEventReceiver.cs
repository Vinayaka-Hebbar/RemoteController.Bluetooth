using RemoteController.Bluetooth;
using RemoteController.Desktop;
using RemoteController.Messages;
using RemoteController.Sockets;
using RemoteController.Win32.Hooks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RemoteController.Core
{
    internal class ServerEventReceiver : IDisposable
    {
        private static readonly Guid ServiceId = new Guid("9bde4762-89a6-418e-bacf-fcd82f1e0677");

        private readonly IGlobalHook _hook;
        private readonly VirtualScreenManager _screen;
        private readonly CancellationTokenSource cts;
        private readonly ClientState state;

#if QUEUE_SERVER
        private readonly ConcurrentQueue<IMessage> messages;
        private readonly EventWaitHandle messageHandle;
#endif

        bool isRunning;
#if QUEUE_SERVER
        public ServerEventReceiver(VirtualScreenManager screen)
        {
            _hook = new WindowsGlobalHook();
            _screen = screen;
            messageHandle = new AutoResetEvent(false);
            messages = new ConcurrentQueue<IMessage>();
            cts = new CancellationTokenSource();
            state = screen.State;
        }
#else
        public ServerEventReceiver(VirtualScreenManager screen)
        {
            _hook = new WindowsGlobalHook();
            _screen = screen;
            cts = new CancellationTokenSource();
            state = screen.State;
        }
#endif

        public void Start()
        {
            isRunning = true;
            VirtualScreen s = null;
            foreach (Display display in _hook.GetDisplays())
            {
                s = state.ScreenConfiguration.AddScreen(display.X, display.Y, display.X, display.Y, display.Width, display.Height, state.ClientName);
            }
            state.LastPositionX = 0;
            state.LastPositionY = 0;
            var reciever = new Task(Receive, cts.Token, creationOptions: TaskCreationOptions.LongRunning);
            reciever.ConfigureAwait(false);
            reciever.Start();
#if QUEUE_SERVER
            var dispatcher = new Task(Dispatch, cts.Token, TaskCreationOptions.LongRunning);
            dispatcher.ConfigureAwait(false);
            dispatcher.Start(); 
#endif
        }

#if QUEUE_SERVER
        void Dispatch()
        {
            while (isRunning)
            {
                if (messageHandle.WaitOne() && isRunning)
                {
                    var count = messages.Count;
                    while (count > 0)
                    {
                        count--;
                        if (messages.TryDequeue(out IMessage message))
                        {
                            switch (message.Type)
                            {
                                case MessageType.MoveScreen:
                                    break;
                                case MessageType.MouseWheel:
                                    OnMouseWheelFromServer(new MouseWheelMessage(message));
                                    break;
                                case MessageType.MouseButton:
                                    OnMouseButtonFromServer(new MouseButtonMessage(message));
                                    break;
                                case MessageType.MouseMove:
                                    OnMouseMoveFromServer(new MouseMoveMessage(message));
                                    break;
                                case MessageType.KeyPress:
                                    OnKeyPressFromServer(new KeyPressMessage(message));
                                    break;
                                case MessageType.Clipboard:
                                    OnClipboardFromServer(new ClipboardMessage(message));
                                    break;
                                case MessageType.CheckIn:
                                    OnScreenConfig(((CheckInMessage)message).Screens);
                                    break;
                            }
                        }
                    }
                }
            }
        } 

        void Receive()
        {
            var token = cts.Token;
            try
            {
                var _listener = new BluetoothListener(ServiceId)
                {
                    ServiceName = "MyService"
                };
                _listener.Start();
                using (token.Register(_listener.Stop))
                {
                    while (true)
                    {
                        using (var client = _listener.AcceptBluetoothClient())
                        {
                            if (token.IsCancellationRequested)
                            {
                                return;
                            }
                            ProcessClient(client); 
                        }
                    }
                }

            }
            catch (SocketException)
            {
                // stoped receiving
            }
        }

        void ProcessClient(BluetoothClient client)
        {
            var stream = client.GetStream();
            while (true)
            {
                var buffer = new byte[8];
                if (stream.Read(buffer, 0, 8) > 0 && isRunning)
                {
                    var message = new MessageInfo(buffer);
                    switch ((MessageType)(message.Type & Message.TypeMask))
                    {
                        case MessageType.MoveScreen:
                        case MessageType.MouseButton:
                            messages.Enqueue(message);
                            break;
                        case MessageType.MouseWheel:
                        case MessageType.MouseMove:
                        case MessageType.KeyPress:
                        case MessageType.Clipboard:
                            messages.Enqueue(MessagePacket.Parse(message, stream));
                            break;
                        case MessageType.CheckIn:
                            CheckInMessage checkIn = CheckInMessage.Parse(message, stream);
                            ScreenConfig(stream);
                            messages.Enqueue(checkIn);
                            break;
                    }
                    messageHandle.Set();
                }
            }
        }

        void ScreenConfig(NetworkStream stream)
        {
            var config = new CheckInMessage(state.ClientName, state.ScreenConfiguration.Screens[state.ClientName]);
            var buffer = config.GetBytes();
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }
#else

        async void Receive()
        {
            var token = cts.Token;
            try
            {
                var _listener = new BluetoothListener(ServiceId)
                {
                    ServiceName = "BluetoothRemoteController"
                };
                _listener.Start();
                using (token.Register(_listener.Stop))
                {
                    while (true)
                    {
                        using (var client = _listener.AcceptBluetoothClient())
                        {
                            if (token.IsCancellationRequested)
                            {
                                return;
                            }
                            await ProcessClientAsync(client);
                        }
                    }
                }

            }
            catch (SocketException)
            {
                // stoped receiving
            }
        }

        async Task ProcessClientAsync(BluetoothClient client)
        {
            var stream = client.GetStream();
            while (true)
            {
                var buffer = new byte[8];
                if (await stream.ReadAsync(buffer, 0, 8) > 0 && isRunning)
                {
                    var message = new MessageInfo(buffer);
                    switch ((MessageType)(message.Type & Message.TypeMask))
                    {
                        case MessageType.MoveScreen:
                            break;
                        case MessageType.MouseWheel:
                            OnMouseWheelFromServer(new MouseWheelMessage(await MessagePacket.ParseAsync(message, stream)));
                            break;
                        case MessageType.MouseButton:
                            OnMouseButtonFromServer(new MouseButtonMessage(message));
                            break;
                        case MessageType.MouseMove:
                            OnMouseMoveFromServer(new MouseMoveMessage(await MessagePacket.ParseAsync(message, stream)));
                            break;
                        case MessageType.KeyPress:
                            OnKeyPressFromServer(new KeyPressMessage(await MessagePacket.ParseAsync(message, stream)));
                            break;
                        case MessageType.Clipboard:
                            OnClipboardFromServer(new ClipboardMessage(await MessagePacket.ParseAsync(message, stream)));
                            break;
                        case MessageType.CheckIn:
                            CheckInMessage checkIn = new CheckInMessage(await MessagePacket.ParseAsync(message, stream));
                            await ScreenConfigASync(stream);
                            OnScreenConfig(checkIn.Screens);
                            break;
                    }
                }
            }
        }

        async Task ScreenConfigASync(NetworkStream stream)
        {
            var config = new CheckInMessage(state.ClientName, state.ScreenConfiguration.Screens[state.ClientName]);
            var buffer = config.GetBytes();
            await stream.WriteAsync(buffer, 0, buffer.Length);
            await stream.FlushAsync();
        }
#endif

#if Bail
        private bool ShouldServerBailKeyboard()
        {
            if ((DateTime.UtcNow - state.LastHookEvent_Keyboard).TotalSeconds < 2)
            {
                return true;
            }
            return false;
        }

        private bool ShouldServerBailMouse()
        {
            if ((DateTime.UtcNow - state.LastHookEvent_Mouse).TotalSeconds < 2)
            {
                return true;
            }

            return false;
        } 
#endif

        private void OnClipboardFromServer(ClipboardMessage message)
        {
            //Console.WriteLine("Received clipboard from server");
            //i received a hook event for a copy from another client within 2 seconds of pressing my own
            //clipboard. 
            //historically this has been happening by a global hook reading my event taps and replaying back over the network
            //in a feedback loop. This should be solved, but i'm leaving this code here as an extra check.
#if Bail
            if (ShouldServerBailKeyboard())
                return;

            state.LastServerEvent_Keyboard = DateTime.UtcNow;
#endif

            _hook.SetClipboard(message.Data);
        }

        private void OnMouseMoveFromServer(MouseMoveMessage message)
        {
#if Bail
            if (ShouldServerBailMouse())
                return;

            state.LastServerEvent_Mouse = DateTime.UtcNow;
#endif


            state.VirtualX = message.VirtualX;
            state.VirtualY = message.VirtualY;

            //send this movement to our virtual screen manager for processing
            if (_screen.ProcessVirtualCoordinatesUpdate(true).MoveMouse)
            {
                _hook.SetMousePos(state.LastPositionX, state.LastPositionY);
            }

        }
        private void OnMouseWheelFromServer(MouseWheelMessage message)
        {
#if Bail
            if (ShouldServerBailMouse())
                return;
            state.LastServerEvent_Mouse = DateTime.UtcNow;
#endif
            //Console.WriteLine("Received mouse wheel from server");
            if (state.CurrentClientFocused)
                _hook.SendMouseWheel(message.DeltaX, message.DeltaY);

        }
        private void OnMouseButtonFromServer(MouseButtonMessage message)
        {
#if Bail
            if (ShouldServerBailMouse())
                return;
            state.LastServerEvent_Mouse = DateTime.UtcNow;
#endif
            //Console.WriteLine("Received mouse down from server: " + button.ToString());
            if (state.CurrentClientFocused)
            {
                if (message.IsDown)
                {
                    _hook.SendMouseDown(message.MouseButton);
                }
                else
                {
                    _hook.SendMouseUp(message.MouseButton);
                }
            }

        }

        private void OnKeyPressFromServer(KeyPressMessage message)
        {
#if Bail
            if (ShouldServerBailMouse())
                return;

            state.LastServerEvent_Keyboard = DateTime.UtcNow;
#endif
            if (state.CurrentClientFocused)
            {
                if (message.IsDown)
                {
                    _hook.SendKeyDown(message.Key);
                }
                else
                {
                    _hook.SendKeyUp(message.Key);
                }
            }
        }

        private void OnScreenConfig(IList<VirtualScreen> screens)
        {
            ScreenConfiguration screenConfiguration = state.ScreenConfiguration;
            foreach (var screen in screens)
            {
                //Console.WriteLine("Screen:"+screen.X+","+screen.Y + ", LocalX:"+screen.LocalX + ", "+screen.LocalY + " , Width:"+screen.Width + " , height:"+screen.Height+", client: "+ screen.Client);
                if (!screenConfiguration.Screens.ContainsKey(screen.Client))
                {
                    screenConfiguration.Screens.TryAdd(screen.Client, new List<VirtualScreen>());
                    screenConfiguration.AddScreenLeft(screenConfiguration.GetFurthestRight(), screen.X, screen.Y, screen.Width, screen.Height, screen.Client);
                }

            }
            if (state.ScreenConfiguration.ValidVirtualCoordinate(state.VirtualX, state.VirtualY) !=
                null)
                return;
            //coordinates are invalid, grab a screen
            var s = state.ScreenConfiguration.GetFurthestLeft();
            state.VirtualX = s.X;
            state.VirtualY = s.Y;
            if (s.Client != state.ClientName)
                return;
            //set this local client to have 0,0 coords. then update the other clients with the new virtual position.
            state.LastPositionX = 0;
            state.LastPositionY = 0;
            _hook.SetMousePos(0, 0);
        }

        private bool disposed;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    isRunning = false;
#if QUEUE_SERVER
                    messageHandle.Set();
                    messageHandle.Dispose(); 
#endif
                    cts.Cancel();
                    cts.Dispose();
                }
                disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
