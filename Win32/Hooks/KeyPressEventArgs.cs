namespace RemoteController.Win32.Hooks
{
    /// <summary>
    /// Defines the event arguments for KeyPress events. Instances of this class are cached:
    /// KeyPressEventArgs should only be used inside the relevant event, unless manually cloned.
    /// </summary>
    public class KeyPressEventArgs : System.ComponentModel.HandledEventArgs
    {
        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="keyChar">The ASCII character that was typed.</param>
        public KeyPressEventArgs(char keyChar)
        {
            KeyChar = keyChar;
        }

        /// <summary>
        /// Gets a <see cref="char"/> that defines the ASCII character that was typed.
        /// </summary>
        public char KeyChar { get; internal set; }
    }
}