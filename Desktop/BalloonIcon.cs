namespace RemoteController.Desktop
{
    ///<summary>
    /// Supported icons for the tray's balloon messages.
    ///</summary>
    public enum BalloonIcon
    {
        /// <summary>
        /// The balloon message is displayed without an icon.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// An information is displayed.
        /// </summary>
        Info = 0x01,

        /// <summary>
        /// A warning is displayed.
        /// </summary>
        Warning = 0x02,

        /// <summary>
        /// An error is displayed.
        /// </summary>
        Error = 0x03
    }
}
