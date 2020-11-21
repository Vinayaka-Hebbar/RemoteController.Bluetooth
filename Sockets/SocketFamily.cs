using System.Net.Sockets;

namespace RemoteController.Sockets
{
    internal static class SocketFamily
    {
        /// <summary>
        /// Bluetooth address.
        /// </summary>
        /// <value>32</value>
        public const AddressFamily Bluetooth = (AddressFamily)32;

        /// <summary>
        /// IrDA address used on some Windows CE platforms (Has a different value to <see cref="AddressFamily">AddressFamily.IrDA</see>).
        /// </summary>
        /// <value>22</value>
        public const AddressFamily Irda = (AddressFamily)22;
    }
}