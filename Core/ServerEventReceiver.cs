using RemoteController.Desktop;
using RemoteController.Messages;
using RemoteController.Sockets;
using RemoteController.Win32.Hooks;
using System;
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

#if Bail
        private static readonly TimeSpan BailSec = TimeSpan.FromSeconds(1);
#endif

#if QUEUE_SERVER
        private readonly System.Collections.Concurrent.ConcurrentQueue<IMessage> messages;
        private readonly EventWaitHandle messageHandle;
#endif

        bool isRunning;
#if QUEUE_SERVER
        public ServerEventReceiver(VirtualScreenManager screen)
        {
            _hook = new WindowsGlobalHook();
            _screen = screen;
            messageHandle = new EventWaitHandle(false, EventResetMode.AutoReset, null);
            messages = new System.Collections.Concurrent.ConcurrentQueue<IMessage>();
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
                s = _screen.ScreenConfiguration.AddScreen(display.X, display.Y, display.X, display.Y, display.Width, display.Height, display.GetDpi(), state.ClientName);
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
                if (messageHandle.WaitOne(-1, false) && isRunning)
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
                                    OnMouseWheelFromServer(message.GetBytes());
                                    break;
                                case MessageType.MouseButton:
                                    OnMouseButtonFromServer(message.GetBytes());
                                    break;
                                case MessageType.MouseMove:
                                    OnMouseMoveFromServer(message.GetBytes());
                                    break;
                                case MessageType.KeyPress:
                                    OnKeyPressFromServer(message.GetBytes());
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

        
        void ProcessClient(Bluetooth.BluetoothClient client)
        {
            var stream = client.GetStream();
            while (true)
            {
                var buffer = new byte[Message.HeaderSize];
                if (stream.Read(buffer, 0, Message.HeaderSize) > 0 && isRunning)
                {
                    var message = new MessageInfo(buffer);
                    switch (message.Type)
                    {
                        case MessageType.MoveScreen:
                        case MessageType.MouseButton:
                        case MessageType.MouseWheel:
                        case MessageType.MouseMove:
                        case MessageType.KeyPress:
                            messages.Enqueue(message);
                            break;
                        case MessageType.Clipboard:
                            messages.Enqueue(MessagePacket.Parse(message, stream));
                            break;
                        case MessageType.CheckIn:
                            CheckInMessage checkIn = new CheckInMessage(MessagePacket.Parse(message, stream));
                            ScreenConfig(stream);
                            messages.Enqueue(checkIn);
                            break;
                    }
                    messageHandle.Set();
                }
            }
        }

#endif

#if SYNC_SERVER || QUEUE_SERVER
        void ScreenConfig(System.IO.Stream stream)
        {
            var config = new CheckInMessage(state.ClientName, _screen.ScreenConfiguration.Screens[state.ClientName]);
            var buffer = config.GetBytes();
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }
#endif

#if SYNC_SERVER || QUEUE_SERVER
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
#endif

#if SYNC_SERVER
        void ProcessClient(Bluetooth.BluetoothClient client)
        {
            var stream = client.GetStream();
            while (true)
            {
                var buffer = new byte[Message.HeaderSize];
                if (stream.Read(buffer, 0, Message.HeaderSize) > 0 && isRunning)
                {
                    switch ((MessageType)(buffer[0] & Message.TypeMask))
                    {
                        case MessageType.MoveScreen:
                            break;
                        case MessageType.MouseWheel:
                            OnMouseWheelFromServer(buffer);
                            break;
                        case MessageType.MouseButton:
                            OnMouseButtonFromServer(buffer);
                            break;
                        case MessageType.MouseMove:
                            OnMouseMoveFromServer(buffer);
                            break;
                        case MessageType.KeyPress:
                            OnKeyPressFromServer(buffer);
                            break;
                        case MessageType.Clipboard:
                            OnClipboardFromServer(new ClipboardMessage(MessagePacket.Parse(new MessageInfo(buffer), stream)));
                            break;
                        case MessageType.CheckIn:
                            CheckInMessage checkIn = new CheckInMessage(MessagePacket.Parse(new MessageInfo(buffer), stream));
                            ScreenConfig(stream);
                            OnScreenConfig(checkIn.Screens);
                            break;
                    }
                }
            }
        }
#endif


#if !QUEUE_SERVER && !SYNC_SERVER
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

        async Task ProcessClientAsync(Bluetooth.BluetoothClient client)
        {
            var stream = client.GetStream();
            while (true)
            {
                var buffer = new byte[Message.HeaderSize];
                if (await stream.ReadAsync(buffer, 0, Message.HeaderSize) > 0 && isRunning)
                {
                    var message = new MessageInfo(buffer);
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

#if Bail && QUEUE_SERVER
        private bool ShouldServerBailKeyboard()
        {
            return (DateTime.UtcNow - state.LastHookEvent_Keyboard) < BailSec;
        }

        private bool ShouldServerBailMouse()
        {
            return (DateTime.UtcNow - state.LastHookEvent_Mouse) < BailSec;
        }
#endif

        private void OnClipboardFromServer(ClipboardMessage message)
        {
            //Console.WriteLine("Received clipboard from server");
            //i received a hook event for a copy from another client within 2 seconds of pressing my own
            //clipboard. 
            //historically this has been happening by a global hook reading my event taps and replaying back over the network
            //in a feedback loop. This should be solved, but i'm leaving this code here as an extra check.
#if Bail && QUEUE_SERVER
            if (ShouldServerBailKeyboard())
                return;

            state.LastServerEvent_Keyboard = DateTime.UtcNow;
#endif

            _hook.SetClipboard(message.Data);
        }

        unsafe void OnMouseMoveFromServer(byte[] message)
        {
#if Bail && QUEUE_SERVER
            if (ShouldServerBailMouse())
                return;

            state.LastServerEvent_Mouse = DateTime.UtcNow;
#endif

            fixed (byte* b = message)
            {
                state.VirtualX = *(int*)(b + 1);
                state.VirtualY = *(int*)(b + 5);
            }
            System.Diagnostics.Debug.WriteLine(state);
            //send this movement to our virtual screen manager for processing
            if (_screen.ProcessVirtualCoordinatesMove(true))
            {
                _hook.SetMousePos(state.LastPositionX, state.LastPositionY);
            }

        }

        unsafe void OnMouseWheelFromServer(byte[] message)
        {
#if Bail && QUEUE_SERVER && QUEUE_SERVER
            if (ShouldServerBailMouse())
                return;
            state.LastServerEvent_Mouse = DateTime.UtcNow;
#endif
            //Console.WriteLine("Received mouse wheel from server");
            if (state.CurrentClientFocused)
            {
                fixed (byte* b = message)
                {
                    _hook.SendMouseWheel(*(int*)(b + 1), *(int*)(b + 5));
                }
            }
        }

        unsafe void OnMouseButtonFromServer(byte[] message)
        {
#if Bail && QUEUE_SERVER
            if (ShouldServerBailMouse())
                return;
            state.LastServerEvent_Mouse = DateTime.UtcNow;
#endif
            //Console.WriteLine("Received mouse down from server: " + button.ToString());
            if (state.CurrentClientFocused)
            {
                fixed (byte* b = message)
                {
                    if ((*(b + 2)) == 1)
                    {
                        _hook.SendMouseDown((MouseButton)(*(b + 1)));
                    }
                    else
                    {
                        _hook.SendMouseUp((MouseButton)(*(b + 1)));
                    }
                }
            }

        }

        unsafe void OnKeyPressFromServer(byte[] message)
        {
#if Bail && QUEUE_SERVER
            if (ShouldServerBailMouse())
                return;

            state.LastServerEvent_Keyboard = DateTime.UtcNow;
#endif
            if (state.CurrentClientFocused)
            {
                fixed (byte* b = message)
                {
                    if (*(int*)(b + 5) == 1)
                    {
                        _hook.SendKeyDown((Key)(*(int*)(b + 1)));
                    }
                    else
                    {
                        _hook.SendKeyUp((Key)(*(int*)(b + 1)));
                    }
                }
            }
        }

        private void OnScreenConfig(IList<VirtualScreen> screens)
        {
            ScreenConfiguration screenConfiguration = _screen.ScreenConfiguration;
            foreach (var screen in screens)
            {
                //Console.WriteLine("Screen:"+screen.X+","+screen.Y + ", LocalX:"+screen.LocalX + ", "+screen.LocalY + " , Width:"+screen.Width + " , height:"+screen.Height+", client: "+ screen.Client);
                if (!screenConfiguration.Screens.ContainsKey(screen.Client))
                {
                    screenConfiguration.Screens.TryAdd(screen.Client, new List<VirtualScreen>());
                    screenConfiguration.AddScreenLeft(screenConfiguration.GetFurthestRight(), screen);
                }

            }
            if (screenConfiguration.ValidVirtualCoordinate(state.VirtualX, state.VirtualY) !=
                null)
                return;
            //coordinates are invalid, grab a screen
            var s = screenConfiguration.GetFurthestLeft();
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
