using RemoteController.Win32.Desktop;
using RemoteController.Win32.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RemoteController.Win32.Hooks
{
    public class WindowsGlobalHook : GlobalHookBase, IDisposable
    {
        public const int WH_KEYBOARD_LL = 13;
        public const int WH_MOUSE_LL = 14;

        private IntPtr _user32LibraryHandle;
        private IntPtr _windowsMouseHookHandle;
        private NativeMethods.HookProc _mouseHookProc;
        private IntPtr _windowsKeyboardHookHandle;
        private NativeMethods.HookProc _keyboardHookProc;

        private NativeMethods.WndProc _messageWindowProc;
        private IntPtr _messageWindowHandle;
        private WNDCLASSEX w;

        public override void Init()
        {
            NativeMethods.SetProcessDPIAware();
        }

        [AllowReversePInvokeCalls]
        private IntPtr WndProc(IntPtr hWnd, uint m, IntPtr wParam, IntPtr lParam)
        {
            WindowsMessages msg = (WindowsMessages)m;

            switch (msg)
            {
                case WindowsMessages.CLIPBOARDUPDATE:
                    string text = GetClipboard();
                    if (text != null)
                        OnClipboardChanged(text);
                    break;
            }
            return NativeMethods.DefWindowProc(hWnd, msg, wParam, lParam);

        }

        //https://stackoverflow.com/questions/44102918/read-system-clipboard-as-stream-instead-of-string
        //TODO: we should check the size of the clipboard and only replicate it below a certain threshold
        const uint CF_UNICODETEXT = 13;
        private string GetClipboard()
        {
            if (!NativeMethods.IsClipboardFormatAvailable(CF_UNICODETEXT))
                return null;
            if (!NativeMethods.OpenClipboard(IntPtr.Zero))
                return null;

            string data = null;
            IntPtr hGlobal = NativeMethods.GetClipboardData(CF_UNICODETEXT);
            if (hGlobal != IntPtr.Zero)
            {
                IntPtr lpwcstr = NativeMethods.GlobalLock(hGlobal);
                if (lpwcstr != IntPtr.Zero)
                {
                    data = Marshal.PtrToStringUni(lpwcstr);
                    NativeMethods.GlobalUnlock(lpwcstr);
                }
            }
            NativeMethods.CloseClipboard();

            return data;
        }
        public override void SetClipboard(string value)
        {

            if (value == null)
                return;

            _ = Clippy.PushStringToClipboard(value);

        }


        public override void Start()
        {

            //https://stackoverflow.com/questions/8980873/implementing-a-win32-message-loop-and-creating-a-window-object-with-p-invoke
            _messageWindowProc = WndProc;
            w = new WNDCLASSEX
            {
                cbSize = (uint)Marshal.SizeOf(typeof(WNDCLASSEX)),
                lpszClassName = "OTB_Message_Watcher_Class",
                lpfnWndProc = _messageWindowProc
            };

            NativeMethods.RegisterClassEx(ref w);
            IntPtr hInstance = NativeMethods.GetModuleHandle(Process.GetCurrentProcess().MainModule.ModuleName);
            //new IntPtr(-3) = MESSAGE ONLY
            _messageWindowHandle = NativeMethods.CreateWindowEx(0, w.lpszClassName, w.lpszClassName, 0, 0, 0, 0, 0, new IntPtr(-3), IntPtr.Zero, hInstance /*Can I make this 0?*/, IntPtr.Zero);
            NativeMethods.AddClipboardFormatListener(_messageWindowHandle);


            _windowsMouseHookHandle = IntPtr.Zero;
            _user32LibraryHandle = IntPtr.Zero;
            _mouseHookProc = LowLevelMouseProc; // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behaviour.

            _windowsKeyboardHookHandle = IntPtr.Zero;
            _keyboardHookProc = LowLevelKeyboardProc; // we must keep alive _hookProc, because GC is not aware about SetWindowsHookEx behaviour.


            _user32LibraryHandle = NativeMethods.LoadLibrary("User32");
            if (_user32LibraryHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to load library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }

            _windowsMouseHookHandle = NativeMethods.SetWindowsHookEx(WH_MOUSE_LL, _mouseHookProc, _user32LibraryHandle, 0);
            if (_windowsMouseHookHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to adjust mouse hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }

            _windowsKeyboardHookHandle = NativeMethods.SetWindowsHookEx(WH_KEYBOARD_LL, _keyboardHookProc, _user32LibraryHandle, 0);
            if (_windowsKeyboardHookHandle == IntPtr.Zero)
            {
                int errorCode = Marshal.GetLastWin32Error();
                throw new Win32Exception(errorCode, $"Failed to adjust keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
            }



            //init initial cursor position
            bool c = NativeMethods.GetCursorPos(out Point p);
            MouseState.Position = new MousePoint(p.X, p.Y);

        }
        public IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool fEatMouse = false;

            int wparamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(Native.MouseState), wparamTyped))
            {
                object o = Marshal.PtrToStructure(lParam, typeof(LowLevelMouseInputEvent));
                LowLevelMouseInputEvent p = (LowLevelMouseInputEvent)o;

                MouseHookEventArgs eventArguments = new MouseHookEventArgs(p, (Native.MouseState)wparamTyped);
                if (eventArguments.MouseState == Native.MouseState.MouseMove)
                {
                    OnMouseMove(eventArguments.MouseData.Point.X, eventArguments.MouseData.Point.Y);
                    fEatMouse = MouseMoveArgs.Handled;
                }

                if (eventArguments.MouseState == Native.MouseState.LeftButtonDown)
                {
                    OnMouseDown(MouseButton.Left);
                    fEatMouse = MouseDownArgs.Handled;
                }

                if (eventArguments.MouseState == Native.MouseState.LeftButtonUp)
                {
                    OnMouseUp(MouseButton.Left);
                    fEatMouse = MouseUpArgs.Handled;
                }

                if (eventArguments.MouseState == Native.MouseState.RightButtonDown)
                {
                    OnMouseDown(MouseButton.Right);
                    fEatMouse = MouseDownArgs.Handled;
                }

                if (eventArguments.MouseState == Native.MouseState.RightButtonUp)
                {
                    OnMouseUp(MouseButton.Right);
                    fEatMouse = MouseUpArgs.Handled;
                }

                if (eventArguments.MouseState == Native.MouseState.MBUTTONDOWN)
                {
                    OnMouseDown(MouseButton.Middle);
                    fEatMouse = MouseDownArgs.Handled;
                }

                if (eventArguments.MouseState == Native.MouseState.MBUTTONUP)
                {
                    OnMouseUp(MouseButton.Middle);
                    fEatMouse = MouseUpArgs.Handled;
                }

                if (eventArguments.MouseState == Native.MouseState.XBUTTONDOWN)
                {
                    uint whichXButton = p.Mousedata >> 16;
                    if (whichXButton == 1)
                    {
                        OnMouseDown(MouseButton.Button1);
                        fEatMouse = MouseDownArgs.Handled;
                    }
                    if (whichXButton == 2)
                    {
                        OnMouseDown(MouseButton.Button2);
                        fEatMouse = MouseDownArgs.Handled;
                    }

                }
                if (eventArguments.MouseState == Native.MouseState.XBUTTONUP)
                {
                    uint whichXButton = p.Mousedata >> 16;
                    if (whichXButton == 1)
                    {
                        OnMouseUp(MouseButton.Button1);
                        fEatMouse = MouseUpArgs.Handled;
                    }
                    if (whichXButton == 2)
                    {
                        OnMouseUp(MouseButton.Button2);
                        fEatMouse = MouseUpArgs.Handled;
                    }

                }

                if (eventArguments.MouseState == Native.MouseState.MouseWheel)
                {
                    int delta = (int)p.Mousedata >> 16;
                    OnMouseWheel(0, delta);
                    fEatMouse = MouseDownArgs.Handled;
                }
                if (eventArguments.MouseState == Native.MouseState.MouseHWheel)
                {
                    int delta = (int)p.Mousedata >> 16;
                    OnMouseWheel(delta, 0);
                    fEatMouse = MouseDownArgs.Handled;
                }
            }

            return fEatMouse ? (IntPtr)1 : NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }



        public override void SetMousePos(double x, double y)
        {
            NativeMethods.SetCursorPos((int)Math.Round(x), (int)Math.Round(y));
        }

        public override MousePoint GetMousePos()
        {
            NativeMethods.GetCursorPos(out Point point);
            return new MousePoint(point.X, point.Y);
        }
        public override void SendMouseWheel(int dx, int dy)
        {
            if (dx == 0 && dy == 0)
                return;

            NativeMethods.INPUT[] inputs;
            if (dx != 0 && dy != 0)
            {
                inputs = new NativeMethods.INPUT[2];
            }
            else
            {
                inputs = new NativeMethods.INPUT[1];
            }

            int i = 0;
            if (dy != 0)
            {
                inputs[i] = new NativeMethods.INPUT
                {
                    type = NativeMethods.INPUT_MOUSE,
                    u = new NativeMethods.InputUnion
                    {
                        mi = new NativeMethods.MOUSEINPUT()
                        {
                            mouseData = (uint)dy,
                            dwFlags = NativeMethods.MOUSEEVENTF_WHEEL,
                            dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                        }
                    }
                };
                i++;
            }

            if (dx != 0)
            {
                inputs[i] = new NativeMethods.INPUT
                {
                    type = NativeMethods.INPUT_MOUSE,
                    u = new NativeMethods.InputUnion
                    {
                        mi = new NativeMethods.MOUSEINPUT()
                        {
                            mouseData = (uint)dx,
                            dwFlags = NativeMethods.MOUSEEVENTF_HWHEEL,
                            dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                        }
                    }
                };

            }



            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));

        }
        public override void SendMouseDown(MouseButton button)
        {
            uint flag = 0;
            uint mouseData = 0;
            switch (button)
            {
                case MouseButton.Left:
                    flag = NativeMethods.MOUSEEVENTF_LEFTDOWN;
                    break;
                case MouseButton.Right:
                    flag = NativeMethods.MOUSEEVENTF_RIGHTDOWN;
                    break;
                case MouseButton.Middle:
                    flag = NativeMethods.MOUSEEVENTF_MIDDLEDOWN;
                    break;
                case MouseButton.Button1:
                    mouseData = NativeMethods.XBUTTON1;
                    flag = NativeMethods.MOUSEEVENTF_XDOWN;
                    break;
                case MouseButton.Button2:
                    mouseData = NativeMethods.XBUTTON2;
                    flag = NativeMethods.MOUSEEVENTF_XDOWN;
                    break;
            }
            NativeMethods.INPUT[] inputs = {
                new NativeMethods.INPUT
                {
                    type = NativeMethods.INPUT_MOUSE,
                    u = new NativeMethods.InputUnion
                    {
                        mi = new NativeMethods.MOUSEINPUT()
                        {
                            mouseData = mouseData,
                            dwFlags = flag,
                            dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                        }
                    }
                }
            };

            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));

        }
        public override void SendMouseUp(MouseButton button)
        {

            uint flag = 0;
            uint mouseData = 0;
            switch (button)
            {
                case MouseButton.Left:
                    flag = NativeMethods.MOUSEEVENTF_LEFTUP;
                    break;
                case MouseButton.Right:
                    flag = NativeMethods.MOUSEEVENTF_RIGHTUP;
                    break;
                case MouseButton.Middle:
                    flag = NativeMethods.MOUSEEVENTF_MIDDLEUP;
                    break;
                case MouseButton.Button1:
                    mouseData = NativeMethods.XBUTTON1;
                    flag = NativeMethods.MOUSEEVENTF_XUP;
                    break;
                case MouseButton.Button2:
                    mouseData = NativeMethods.XBUTTON2;
                    flag = NativeMethods.MOUSEEVENTF_XUP;
                    break;
            }
            NativeMethods.INPUT[] inputs = new NativeMethods.INPUT[]
            {
                new NativeMethods.INPUT
                {
                    type = NativeMethods.INPUT_MOUSE,
                    u = new NativeMethods.InputUnion
                    {
                        mi = new NativeMethods.MOUSEINPUT()
                        {
                            mouseData = mouseData,
                            dwFlags = flag,
                            dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                        }
                    }
                }
            };

            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));


        }

        public override void SendKeyDown(Key key)
        {
            bool keyup = false;
            bool altDown = KeyboardState.IsKeyDown(Key.AltLeft) || KeyboardState.IsKeyDown(Key.AltRight);
            WinKeyMap.ReverseTranslateKey(key, keyup, altDown, out int tscancode, out VirtualKeys tvk, out int _, out bool extended);

            // bool sysKey = (!altDown && key == Key.AltLeft) || (!altDown && key == Key.AltRight) || ((key != Key.AltLeft && key != Key.AltRight && altDown));

            int dwFlags = 0x0008;
            if (extended)
                dwFlags |= 0x0001;


            //var altdown = ((tflags) & ((int)KeyFlags.KF_ALTDOWN >> 8)) > 0;
            //var dlgmode = ((tflags) & ((int)KeyFlags.KF_DLGMODE >> 8)) > 0;
            //var menumode = ((tflags) & ((int)KeyFlags.KF_MENUMODE >> 8)) > 0;
            //var repeat = ((tflags) & ((int)KeyFlags.KF_REPEAT >> 8)) > 0;
            //var up = ((tflags) & ((int)KeyFlags.KF_UP >> 8)) > 0;



            //a lot of this needs to be thrown away....
            KeyboardState.SetKeyState(key, true);

            NativeMethods.INPUT[] inputs;
            if (extended)
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan =  0xe0,
                                wVk =  0,
                                dwFlags =  0,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    },
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }
            else
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }

            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }
        public override void SendKeyUp(Key key)
        {
            bool keyup = true;
            bool altDown = KeyboardState.IsKeyDown(Key.AltLeft) || KeyboardState.IsKeyDown(Key.AltRight);

            WinKeyMap.ReverseTranslateKey(key, keyup, altDown, out int tscancode, out VirtualKeys tvk, out _, out bool extended);

            // bool sysKey = (!altDown && key == Key.AltLeft) || (!altDown && key == Key.AltRight) || ((key != Key.AltLeft && key != Key.AltRight && altDown));

            int dwFlags = 0x0008 | 0x0002;

            if (extended)
                dwFlags |= 0x0001;


            KeyboardState.SetKeyState(key, false);

            //var altdown = ((tflags) & ((int)KeyFlags.KF_ALTDOWN >> 8)) > 0;
            //var dlgmode = ((tflags) & ((int)KeyFlags.KF_DLGMODE >> 8)) > 0;
            //var menumode = ((tflags) & ((int)KeyFlags.KF_MENUMODE >> 8)) > 0;
            //var repeat = ((tflags) & ((int)KeyFlags.KF_REPEAT >> 8)) > 0;
            //var up = ((tflags) & ((int)KeyFlags.KF_UP >> 8)) > 0;



            NativeMethods.INPUT[] inputs;
            if (extended)
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan =  0xe0,
                                wVk =  0,
                                dwFlags =   0,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    },
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }
            else
            {
                inputs = new[]
                {
                    new NativeMethods.INPUT
                    {
                        type = NativeMethods.INPUT_KEYBOARD,

                        u = new NativeMethods.InputUnion
                        {
                            ki = new NativeMethods.KEYBDINPUT()
                            {
                                wScan = (ushort) tscancode,
                                wVk = (ushort) tvk,
                                dwFlags = (ushort) dwFlags,
                                dwExtraInfo = NativeMethods.GetMessageExtraInfo()
                            }
                        }
                    }
                };
            }

            NativeMethods.SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(NativeMethods.INPUT)));
        }




        public override IList<Display> GetDisplays()
        {

            List<Display> displays = new List<Display>();
            NativeMethods.EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero,
                delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
                {
                    MONITORINFO mi = new MONITORINFO();
                    mi.size = (uint)Marshal.SizeOf(mi);
                    NativeMethods.GetMonitorInfo(hMonitor, ref mi);

                    int w = mi.monitor.right - mi.monitor.left;
                    int h = mi.monitor.bottom - mi.monitor.top;
                    displays.Add(new Display(mi.monitor.left, mi.monitor.top, w, h));

                    return true;
                }, IntPtr.Zero);
            return displays;
        }



        public IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam)
        {

            bool fEatKeyStroke = false;

            int wparamTyped = wParam.ToInt32();
            if (Enum.IsDefined(typeof(Native.KeyboardState), wparamTyped))
            {
                object o = Marshal.PtrToStructure(lParam, typeof(LowLevelKeyboardInputEvent));
                LowLevelKeyboardInputEvent p = (LowLevelKeyboardInputEvent)o;

                KeyboardHookEventArgs eventArguments = new KeyboardHookEventArgs(p, (Native.KeyboardState)wparamTyped);

                int scancode = eventArguments.KeyboardData.HardwareScanCode;
                VirtualKeys vkey = (VirtualKeys)eventArguments.KeyboardData.VirtualCode;

                int flags = eventArguments.KeyboardData.Flags;
                bool extended = ((flags) & ((int)KeyFlags.KF_EXTENDED >> 8)) > 0;

                //var altdown = ((flags) & ((int)KeyFlags.KF_ALTDOWN >> 8)) > 0;
                //var dlgmode = ((flags) & ((int)KeyFlags.KF_DLGMODE >> 8)) > 0;
                //var menumode = ((flags) & ((int)KeyFlags.KF_MENUMODE >> 8)) > 0;
                //var repeat = ((flags) & ((int)KeyFlags.KF_REPEAT >> 8)) > 0;
                //var up = ((flags) & ((int)KeyFlags.KF_UP >> 8)) > 0;


                //TODO: why is this code ignoring virtual keys and mapping it custom?
                //TODO: figure out what extended 2 is supposed to do from the raw input opentk code....
                Key key = WinKeyMap.TranslateKey(scancode, vkey, extended, false, out bool is_valid);



                if (is_valid)
                {

                    if (eventArguments.KeyboardState == Native.KeyboardState.KeyDown || eventArguments.KeyboardState == Native.KeyboardState.SysKeyDown)
                    {
                        OnKeyDown(key);
                        fEatKeyStroke = KeyDownArgs.Handled;
                    }
                    if (eventArguments.KeyboardState == Native.KeyboardState.KeyUp || eventArguments.KeyboardState == Native.KeyboardState.SysKeyUp)
                    {
                        OnKeyUp(key);
                        fEatKeyStroke = KeyUpArgs.Handled;
                    }
                }



            }

            return fEatKeyStroke ? (IntPtr)1 : NativeMethods.CallNextHookEx(IntPtr.Zero, nCode, wParam, lParam);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_messageWindowHandle != IntPtr.Zero)
                {
                    NativeMethods.RemoveClipboardFormatListener(_messageWindowHandle);
                    NativeMethods.DestroyWindow(_messageWindowHandle);
                    NativeMethods.UnregisterClass("OTB_Message_Watcher_Class", IntPtr.Zero);
                }
                // because we can unhook only in the same thread, not in garbage collector thread
                if (_windowsMouseHookHandle != IntPtr.Zero)
                {

                    if (!NativeMethods.UnhookWindowsHookEx(_windowsMouseHookHandle))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode, $"Failed to remove mouse hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                    }
                    _windowsMouseHookHandle = IntPtr.Zero;

                    // ReSharper disable once DelegateSubtraction
                    _mouseHookProc -= LowLevelMouseProc;
                }

                // because we can unhook only in the same thread, not in garbage collector thread
                if (_windowsKeyboardHookHandle != IntPtr.Zero)
                {
                    if (!NativeMethods.UnhookWindowsHookEx(_windowsKeyboardHookHandle))
                    {
                        int errorCode = Marshal.GetLastWin32Error();
                        throw new Win32Exception(errorCode, $"Failed to remove keyboard hooks for '{Process.GetCurrentProcess().ProcessName}'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                    }
                    _windowsKeyboardHookHandle = IntPtr.Zero;

                    // ReSharper disable once DelegateSubtraction
                    _keyboardHookProc -= LowLevelKeyboardProc;
                }
            }

            if (_user32LibraryHandle != IntPtr.Zero)
            {
                if (!NativeMethods.FreeLibrary(_user32LibraryHandle)) // reduces reference to library by 1.
                {
                    int errorCode = Marshal.GetLastWin32Error();
                    throw new Win32Exception(errorCode, $"Failed to unload library 'User32.dll'. Error {errorCode}: {new Win32Exception(Marshal.GetLastWin32Error()).Message}.");
                }
                _user32LibraryHandle = IntPtr.Zero;
            }
        }

        ~WindowsGlobalHook()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
