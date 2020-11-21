﻿namespace RemoteController.Win32.Hooks
{
    /// <summary>
    /// Defines the event data for <see cref="KeyboardDevice"/> events.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Do not cache instances of this type outside their event handler.
    /// If necessary, you can clone a KeyboardEventArgs instance using the
    /// <see cref="KeyboardKeyEventArgs(KeyboardKeyEventArgs)"/> constructor.
    /// </para>
    /// </remarks>
    public class KeyboardKeyEventArgs : System.ComponentModel.HandledEventArgs
    {
        /// <summary>
        /// Constructs a new KeyboardEventArgs instance.
        /// </summary>
        public KeyboardKeyEventArgs() { }

        /// <summary>
        /// Constructs a new KeyboardEventArgs instance.
        /// </summary>
        /// <param name="args">An existing KeyboardEventArgs instance to clone.</param>
        public KeyboardKeyEventArgs(KeyboardKeyEventArgs args)
        {
            Key = args.Key;
        }

        /// <summary>
        /// Gets the <see cref="Key"/> that generated this event.
        /// </summary>
        public Key Key { get; internal set; }

        /// <summary>
        /// Gets the scancode which generated this event.
        /// </summary>        
        public uint ScanCode
        {
            get { return (uint)Key; }
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="KeyModifiers.Alt"/> is pressed.
        /// </summary>
        /// <value><c>true</c> if pressed; otherwise, <c>false</c>.</value>
        public bool Alt
        {
            get { return Keyboard[Key.AltLeft] || Keyboard[Key.AltRight]; }
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="KeyModifiers.Control"/> is pressed.
        /// </summary>
        /// <value><c>true</c> if pressed; otherwise, <c>false</c>.</value>
        public bool Control
        {
            get { return Keyboard[Key.ControlLeft] || Keyboard[Key.ControlRight]; }
        }

        /// <summary>
        /// Gets a value indicating whether <see cref="KeyModifiers.Shift"/> is pressed.
        /// </summary>
        /// <value><c>true</c> if pressed; otherwise, <c>false</c>.</value>
        public bool Shift
        {
            get { return Keyboard[Key.ShiftLeft] || Keyboard[Key.ShiftRight]; }
        }

        /// <summary>
        /// Gets a bitwise combination representing the <see cref="KeyModifiers"/>
        /// that are currently pressed.
        /// </summary>
        /// <value>The modifiers.</value>
        public KeyModifiers Modifiers
        {
            get
            {
                KeyModifiers mods = 0;
                mods |= Alt ? KeyModifiers.Alt : 0;
                mods |= Control ? KeyModifiers.Control : 0;
                mods |= Shift ? KeyModifiers.Shift : 0;
                return mods;
            }
        }

        /// <summary>
        /// Gets the current <see cref="KeyboardState"/>.
        /// </summary>
        /// <value>The keyboard.</value>
        public KeyboardState Keyboard { get; internal set; }
    }
}