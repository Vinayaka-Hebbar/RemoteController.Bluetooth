﻿using RemoteController.Desktop;
using RemoteController.Messages;
using RemoteController.Win32.Hooks;
using System;
using System.Collections.Generic;

namespace RemoteController.Core
{
    public class HookManager : IDisposable
    {
        private readonly ServerEventDispatcher _dispatcher;
        private readonly VirtualScreenManager _screen;
        public readonly IGlobalHook Hook;

        private readonly ClientState ClientState;

        public HookManager(ServerEventDispatcher dispatcher, VirtualScreenManager screen)
        {
            _dispatcher = dispatcher;
            _screen = screen;
            Hook = new WindowsGlobalHook();
            ClientState = screen.State;
        }
        //On some platforms this does some setup work needed - like settings the process DPI aware
        public void Init()
        {
            Hook.Init();

        }
        public void Start()
        {
            Hook.Start();
            Hook.MouseMove += OnGlobalHookMouseMove;
            Hook.MouseDown += OnGlobalHookMouseDown;
            Hook.MouseWheel += OnGlobalHookMouseWheel;
            Hook.MouseUp += OnGlobalHookMouseUp;
            Hook.KeyDown += OnGlobalHookKeyDown;
            Hook.KeyUp += OnGlobalHookKeyUp;
            Hook.Clipboard += OnGlobalHookClipboard;


            MousePoint c = Hook.GetMousePos();

            ClientState.VirtualX = c.X;
            ClientState.VirtualY = c.Y;
            ClientState.LastPositionX = c.X;
            ClientState.LastPositionY = c.Y;
        }

        public void Stop()
        {
            Hook.MouseMove -= OnGlobalHookMouseMove;
            Hook.MouseDown -= OnGlobalHookMouseDown;
            Hook.MouseWheel -= OnGlobalHookMouseWheel;
            Hook.MouseUp -= OnGlobalHookMouseUp;
            Hook.KeyDown -= OnGlobalHookKeyDown;
            Hook.KeyUp -= OnGlobalHookKeyUp;
            Hook.Clipboard -= OnGlobalHookClipboard;
        }

        private void OnGlobalHookClipboard(object sender, ClipboardChangedEventArgs e)
        {
            if (!ClientState.CurrentClientFocused)
            {
                //Console.WriteLine("Trying to set the clipboard when we're not the current client...");
                return;
            }

            //if our application has received a clipboard push from the server, this event still fires, so bail out if we are currently syncing the clipboard.
            //don't process a hook event within 2 seconds 
#if Bail
            if (ShouldHookBailKeyboard())
                return; 
#endif

            ClientState.LastHookEvent_Keyboard = DateTime.UtcNow;
            //Console.WriteLine("Sending clipboard to server");

            _dispatcher.Process(new ClipboardMessage(e.Value));
        }

        private void OnGlobalHookMouseWheel(object sender, MouseWheelEventArgs e)
        {
            if (ClientState.CurrentClientFocused)
            {
                e.Handled = true;
                return;
            }

            //don't process a hook event within 2 seconds of receiving network events. 
#if Bail
            if (ShouldHookBailMouse())
                return; 
#endif
            ClientState.LastHookEvent_Mouse = DateTime.UtcNow;
            _dispatcher.Process(new MouseWheelMessage(e.DeltaX, e.DeltaY));

        }
        private void OnGlobalHookMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ClientState.CurrentClientFocused)
            {
                e.Handled = true;
                return;
            }
            //don't process a hook event within 2 seconds 
#if Bail
            if (ShouldHookBailMouse())
                return; 
#endif
            ClientState.LastHookEvent_Mouse = DateTime.UtcNow;
            _dispatcher.Process(new MouseButtonMessage(e.Button, true));
        }

        private void OnGlobalHookMouseUp(object sender, MouseButtonEventArgs e)
        {
            if (ClientState.CurrentClientFocused)
            {
                e.Handled = true;
                return;
            }
            //don't process a hook event within 2 seconds 
#if Bail
            if (ShouldHookBailMouse())
                return; 
#endif
            ClientState.LastHookEvent_Mouse = DateTime.UtcNow;
            _dispatcher.Process(new MouseButtonMessage(e.Button, false));

        }

        private void OnGlobalHookMouseMove(object sender, MouseMoveEventArgs e)
        {
            //don't process a hook event within 2 seconds of a server event
#if Bail
            if (ShouldHookBailMouse())
                return; 
#endif

            ClientState.LastHookEvent_Mouse = DateTime.UtcNow;

            CoordinateCalculationResult result = _screen.UpdateVirtualMouseCoordinates(e);
            if (result == CoordinateCalculationResult.Valid)
            {
                CoordinateUpdateResult presult = _screen.ProcessVirtualCoordinatesUpdate();
                if (presult.MoveMouse)
                {
                    //Console.WriteLine("Moving mouse to a position");
                    Hook.SetMousePos(ClientState.LastPositionX, ClientState.LastPositionY);
                }

                if (presult.HandleEvent) //we are receiving local input, but mouse is on a virtual monitor. We need to lock the cursor in a position.
                {
                    e.Handled = true; //windows obeys this
                }

                //send over the net
                _dispatcher.Process(new MouseMoveMessage(ClientState.VirtualX, ClientState.VirtualY));
            }
            else if (!ClientState.CurrentClientFocused)
            {
                //if we're the current client, i'm letting this through to enable smoother scrolling along edges.

                //if we're not the current client, handle it.
                e.Handled = true;
            }

        }

        private void OnGlobalHookKeyDown(object sender, KeyboardKeyEventArgs e)
        {
            //TODO: remove this
            //I put this here for when I wanted a fail-safe bailout :)
            if (e.Key == Key.Tilde)
                Environment.Exit(-1);
            if (ClientState.CurrentClientFocused)
            {
                e.Handled = true;
                return;
            }

#if Bail
            if (ShouldHookBailKeyboard())
                return;

#endif
            ClientState.LastHookEvent_Keyboard = DateTime.UtcNow;
            _dispatcher.Process(new KeyPressMessage(e.Key, true));

        }

        private void OnGlobalHookKeyUp(object sender, KeyboardKeyEventArgs e)
        {
            if (!ClientState.CurrentClientFocused)
                e.Handled = true;

#if Bail
            if (ShouldHookBailKeyboard())
                return; 
#endif

            ClientState.LastHookEvent_Keyboard = DateTime.UtcNow;
            _dispatcher.Process(new KeyPressMessage(e.Key, false));

        }

#if Bail
        private bool ShouldHookBailKeyboard()
        {
            if ((DateTime.UtcNow - ClientState.LastServerEvent_Keyboard).TotalSeconds < 1)
                return true;

            return false;
        }

        bool ShouldHookBailMouse()
        {
            if ((DateTime.UtcNow - ClientState.LastServerEvent_Mouse).TotalSeconds < 1)
                return true;
            return false;
        } 
#endif

        public void Dispose()
        {
            Stop();
            Hook.Dispose();
        }

        public IList<Display> GetDisplays()
        {
            return Hook.GetDisplays();

        }
    }
}