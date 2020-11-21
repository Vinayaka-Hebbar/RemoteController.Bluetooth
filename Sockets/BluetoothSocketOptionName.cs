using System.Net.Sockets;

namespace RemoteController.Sockets
{
    /// <summary>
    /// Defines <see cref="Socket"/> configuration option names for the <see cref="Socket"/> class.
    /// </summary>
    public static class BluetoothSocketOptionName
    {
        /// <summary>
        /// Toggles authentication under Windows.
        /// </summary>
        /// <remarks>optlen=sizeof(ULONG), optval = &amp;(ULONG)TRUE/FALSE</remarks>
        public const SocketOptionName Authenticate = unchecked((SocketOptionName)0x80000001); // optlen=sizeof(ULONG), optval = &(ULONG)TRUE/FALSE 

        /// <summary>
        /// On a connected socket, this command turns encryption on or off.
        /// On an unconnected socket, this forces encryption to be on or off on connection.
        /// For an incoming connection, this means that the connection is rejected if the encryption cannot be turned on.
        /// </summary>
        public const SocketOptionName Encrypt = (SocketOptionName)0x00000002; // optlen=sizeof(unsigned int), optval = &amp;(unsigned int)TRUE/FALSE

        /// <summary>
        /// Get or set the default MTU on Windows.
        /// </summary>
        /// <remarks>optlen=sizeof(ULONG), optval = &amp;mtu</remarks>
        public const SocketOptionName Mtu = unchecked((SocketOptionName)0x80000007); // optlen=sizeof(ULONG), optval = &mtu

        /// <summary>
        /// Get or set the maximum MTU on Windows.
        /// </summary>
        /// <remarks>optlen=sizeof(ULONG), optval = &amp;max. mtu</remarks>
        public const SocketOptionName MtuMaximum = unchecked((SocketOptionName)0x80000008);// - 2147483640;

        /// <summary>
        /// Get or set the minimum MTU on Windows.
        /// </summary>
        /// <remarks>optlen=sizeof(ULONG), optval = &amp;min. mtu</remarks>
        public const SocketOptionName MtuMinimum = unchecked((SocketOptionName)0x8000000a);// - 2147483638;
    }
}