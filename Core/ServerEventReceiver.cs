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
        private readonly EventWaitHandle messageHandle;
        private readonly CancellationTokenSource cts;
        private readonly ClientState state;

        private readonly ConcurrentQueue<IMessage> messages;

        bool isRunning;
        public ServerEventReceiver(VirtualScreenManager screen)
        {
            _hook = new WindowsGlobalHook();
            _screen = screen;
            messageHandle = new AutoResetEvent(false);
            messages = new ConcurrentQueue<IMessage>();
            cts = new CancellationTokenSource();
            state = screen.State;
        }

        public void Start()
        {
            isRunning = true;
            var reciever = new Task(Receive, cts.Token, creationOptions: TaskCreationOptions.LongRunning);
            reciever.ConfigureAwait(false);
            reciever.Start();
            var dispatcher = new Task(Dispatch, cts.Token, TaskCreationOptions.LongRunning);
            dispatcher.ConfigureAwait(false);
            dispatcher.Start();
        }

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
                                    OnMouseWheelFromServer((MouseWheelMessage)message);
                                    break;
                                case MessageType.MouseButton:
                                    OnMouseButtonFromServer((MouseButtonMessage)message);
                                    break;
                                case MessageType.MouseMove:
                                    OnMouseMoveFromServer((MouseMoveMessage)message);
                                    break;
                                case MessageType.KeyPress:
                                    OnKeyPressFromServer((KeyPressMessage)message);
                                    break;
                                case MessageType.Clipboard:
                                    OnClipboardFromServer(((ClipboardMessage)message).Data);
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
            var _listener = new BluetoothListener(ServiceId)
            {
                ServiceName = "MyService"
            };
            _listener.Start();
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

        void ProcessClient(Bluetooth.BluetoothClient client)
        {
            var stream = client.GetStream();
            try
            {
                while (true)
                {
                    var buffer = new byte[8];
                    if (stream.Read(buffer, 0, 8) > 0)
                    {
                        var message = new MessageInfo(buffer);
                        switch ((MessageType)(message.Type & Message.TypeMask))
                        {
                            case MessageType.MoveScreen:
                                messages.Enqueue(message);
                                break;
                            case MessageType.MouseWheel:
                                messages.Enqueue(MouseWheelMessage.Parse(message, stream));
                                break;
                            case MessageType.MouseButton:
                                messages.Enqueue(message);
                                break;
                            case MessageType.MouseMove:
                                messages.Enqueue(MouseMoveMessage.Parse(message, stream));
                                break;
                            case MessageType.KeyPress:
                                messages.Enqueue(KeyPressMessage.Parse(message, stream));
                                break;
                            case MessageType.Clipboard:
                                messages.Enqueue(ClipboardMessage.Parse(message, stream));
                                break;
                            case MessageType.CheckIn:
                                messages.Enqueue(CheckInMessage.Parse(message, stream));
                                break;
                        }
                        messageHandle.Set();
                    }
                }
            }
            catch (SocketException)
            {

                throw;
            }
        }

        private bool ShouldServerBailKeyboard()
        {
            if ((DateTime.Now - state.LastHookEvent_Keyboard).TotalSeconds < 2)
            {
                return true;
            }
            return false;
        }
        private bool ShouldServerBailMouse()
        {
            if ((DateTime.Now - state.LastHookEvent_Mouse).TotalSeconds < 2)
            {
                return true;
            }

            return false;
        }

        private void OnClipboardFromServer(string value)
        {
            //Console.WriteLine("Received clipboard from server");
            //i received a hook event for a copy from another client within 2 seconds of pressing my own
            //clipboard. 
            //historically this has been happening by a global hook reading my event taps and replaying back over the network
            //in a feedback loop. This should be solved, but i'm leaving this code here as an extra check.
            if (ShouldServerBailKeyboard())
                return;

            state.LastServerEvent_Keyboard = DateTime.Now;

            _hook.SetClipboard(value);

        }
        private void OnMouseMoveFromServer(MouseMoveMessage message)
        {
            if (ShouldServerBailMouse())
                return;

            state.LastServerEvent_Mouse = DateTime.Now;


            state.VirtualX = message.VirtualX;
            state.VirtualY = message.VirtualY;

            //send this movement to our virtual screen manager for processing
            var result = _screen.ProcessVirtualCoordinatesUpdate(true);
            if (result.MoveMouse)
            {
                _hook.SetMousePos(state.LastPositionX, state.LastPositionY);
            }

            if (result.HandleEvent)
            {

            }

        }
        private void OnMouseWheelFromServer(MouseWheelMessage message)
        {
            if (ShouldServerBailMouse())
                return;

            state.LastServerEvent_Mouse = DateTime.Now;
            //Console.WriteLine("Received mouse wheel from server");
            if (state.CurrentClientFocused)
                _hook.SendMouseWheel(message.DeltaX, message.DeltaY);

        }
        private void OnMouseButtonFromServer(MouseButtonMessage message)
        {
            if (ShouldServerBailMouse())
                return;

            state.LastServerEvent_Mouse = DateTime.Now;
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
            if (ShouldServerBailKeyboard())
                return;

            state.LastServerEvent_Keyboard = DateTime.Now;
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

            state.ScreenConfiguration.Screens = new ConcurrentDictionary<string, List<VirtualScreen>>();

            foreach (var screen in screens)
            {
                //Console.WriteLine("Screen:"+screen.X+","+screen.Y + ", LocalX:"+screen.LocalX + ", "+screen.LocalY + " , Width:"+screen.Width + " , height:"+screen.Height+", client: "+ screen.Client);
                if (!state.ScreenConfiguration.Screens.ContainsKey(screen.Client))
                {
                    state.ScreenConfiguration.Screens.TryAdd(screen.Client, new List<VirtualScreen>());
                }
                state.ScreenConfiguration.Screens[screen.Client].Add(screen);

            }

            if (state.ScreenConfiguration.ValidVirtualCoordinate(state.VirtualX, state.VirtualY) !=
                null) return;
            //coordinates are invalid, grab a screen
            var s = state.ScreenConfiguration.GetFurthestLeft();
            state.VirtualX = s.X;
            state.VirtualY = s.Y;
            if (s.Client != state.ClientName) return;
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
                    messageHandle.Set();
                    messageHandle.Dispose();
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
