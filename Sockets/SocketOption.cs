using RemoteController.Bluetooth;
using System.Net.Sockets;

namespace RemoteController.Sockets
{
    public class SocketOption
    {
        readonly Socket m_socket;
        private bool authenticate = false;
        private bool encrypt = false;
        private BluetoothAuthentication m_authenticator;

        internal SocketOption(Socket socket)
        {
            m_socket = socket;
        }

        #region Authenticate
        /// <summary>
        /// Gets or sets the authentication state of the current connect or behaviour to use when connection is established.
        /// </summary>
        /// <remarks>
        /// For disconnected sockets, specifies that authentication is required in order for a connect or accept operation to complete successfully.
        /// Setting this option actively initiates authentication during connection establishment, if the two Bluetooth devices were not previously authenticated.
        /// The user interface for passkey exchange, if necessary, is provided by the operating system outside the application context.
        /// For outgoing connections that require authentication, the connect operation fails with WSAEACCES if authentication is not successful.
        /// In response, the application may prompt the user to authenticate the two Bluetooth devices before connection.
        /// For incoming connections, the connection is rejected if authentication cannot be established and returns a WSAEHOSTDOWN error.
        /// </remarks>
        public bool Authenticate
        {
            get
            {
                return authenticate;
            }
            set
            {
#if NETCF
                    m_socket.SetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.SetAuthenticationEnabled, (int)(value ? 1 : 0));
#else
                m_socket.SetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.Authenticate, value);
                authenticate = value;
#endif
            }
        }
        #endregion

        #region Encrypt
        /// <summary>
        /// On unconnected sockets, enforces encryption to establish a connection.
        /// Encryption is only available for authenticated connections.
        /// For incoming connections, a connection for which encryption cannot be established is automatically rejected and returns WSAEHOSTDOWN as the error.
        /// For outgoing connections, the connect function fails with WSAEACCES if encryption cannot be established.
        /// In response, the application may prompt the user to authenticate the two Bluetooth devices before connection.
        /// </summary>
        public bool Encrypt
        {
            get { return encrypt; }
            set
            {
                m_socket.SetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.Encrypt, value ? 1 : 0);
                encrypt = value;
            }
        }
        #endregion

        #region Set Pin
        public void SetPin(BluetoothAddress device, string pin)
        {
#if WinXP
            if (pin != null)
            {
                m_authenticator = new BluetoothAuthentication(device, pin);
            }
            else
            {
                if (m_authenticator != null)
                {
                    m_authenticator.Dispose();
                }
            }
#else
            byte[] link = new byte[32];

            //copy remote device address
            if (device != null)
            {
                Buffer.BlockCopy(device.ToByteArray(), 0, link, 8, 6);
            }

            //copy PIN
            if (pin != null & pin.Length > 0)
            {
                if (pin.Length > 16)
                {
                    throw new ArgumentOutOfRangeException("PIN must be between 1 and 16 ASCII characters");
                }
                //copy pin bytes
                byte[] pinbytes = System.Text.Encoding.ASCII.GetBytes(pin);
                Buffer.BlockCopy(pinbytes, 0, link, 16, pin.Length);
                BitConverter.GetBytes(pin.Length).CopyTo(link, 0);
            }

            m_socket.SetSocketOption(BluetoothSocketOptionLevel.RFComm, BluetoothSocketOptionName.SetPin, link);
#endif
        }
        #endregion

    }
}