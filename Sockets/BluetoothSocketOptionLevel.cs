using System.Net.Sockets;

namespace RemoteController.Sockets
{
    public static class BluetoothSocketOptionLevel
    {
        /// <summary>
        /// Bluetooth RFComm protocol (bt-rfcomm)
        /// </summary>
        public const SocketOptionLevel RFComm = (SocketOptionLevel)0x03;
        /// <summary>
        /// Logical Link Control and Adaptation Protocol (bt-l2cap)
        /// </summary>
        public const SocketOptionLevel L2Cap = (SocketOptionLevel)0x100;
        /// <summary>
        /// Service Discovery Protocol (bt-sdp)
        /// </summary>
        public const SocketOptionLevel Sdp = (SocketOptionLevel)0x0101;
    }
}