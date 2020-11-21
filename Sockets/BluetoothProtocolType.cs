using System.Net.Sockets;

namespace RemoteController.Sockets
{
    /// <summary>
    /// Specifies additional protocols that the <see cref="Socket"/> class supports.
    /// </summary>
    /// <remarks>
    /// <para>These constants are defined by the Bluetooth SIG - <see href="https://www.bluetooth.org/Technical/AssignedNumbers/service_discovery.htm"/>
    /// </para>
    /// </remarks>
    public static class BluetoothProtocolType
    {
        /// <summary>
        /// Service Discovery Protocol (bt-sdp)
        /// </summary>
        public const ProtocolType Sdp = (ProtocolType)0x0001;
        /*
        /// <summary>
        /// 
        /// </summary>
        public const ProtocolType Udp = (ProtocolType)0x0002;*/

        /// <summary>
        /// Bluetooth RFComm protocol (bt-rfcomm)
        /// </summary>
        public const ProtocolType RFComm = (ProtocolType)0x0003;

        /// <summary>
        /// Logical Link Control and Adaptation Protocol (bt-l2cap)
        /// </summary>
        public const ProtocolType L2Cap = (ProtocolType)0x0100;
    }
}