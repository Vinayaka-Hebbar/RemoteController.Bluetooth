using RemoteController.Sockets;
using RemoteController.Win32;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace RemoteController.Bluetooth
{
    public class BluetoothClient : IDisposable
    {
        private readonly SocketOption option;
        string m_pinForConnect;

        public BluetoothClient()
        {
            _socket = new BluetoothSocket();
            option = new SocketOption(Socket);
        }

        public BluetoothClient(BluetoothEndPoint endPoint) : this()
        {
            if (endPoint == null)
            {
                throw new ArgumentNullException("localEP");
            }
            Socket.Bind(endPoint);
        }

        public BluetoothClient(Socket socket)
        {
            _socket = socket;
            option = new SocketOption(socket);
        }

        /// <summary>
        /// Returns a collection of paired devices.
        /// </summary>
        public IEnumerable<BluetoothDeviceInfo> PairedDevices
        {
            get
            {
                BLUETOOTH_DEVICE_SEARCH_PARAMS search = BLUETOOTH_DEVICE_SEARCH_PARAMS.Create();
                search.cTimeoutMultiplier = 8;
                search.fReturnAuthenticated = true;
                search.fReturnRemembered = false;
                search.fReturnUnknown = false;
                search.fReturnConnected = false;
                search.fIssueInquiry = false;

                BLUETOOTH_DEVICE_INFO device = BLUETOOTH_DEVICE_INFO.Create();
                IntPtr searchHandle = NativeMethods.BluetoothFindFirstDevice(ref search, ref device);
                if (searchHandle != IntPtr.Zero)
                {
                    yield return new BluetoothDeviceInfo(device);

                    while (NativeMethods.BluetoothFindNextDevice(searchHandle, ref device))
                    {
                        yield return new BluetoothDeviceInfo(device);
                    }

                    NativeMethods.BluetoothFindDeviceClose(searchHandle);
                }

                yield break;
            }
        }

        /// <summary>
        /// Discovers accessible Bluetooth devices, and returns their names and addresses.
        /// </summary>
        /// <returns>An array of BluetoothDeviceInfo objects describing the devices discovered.</returns>
        public static IReadOnlyList<BluetoothDeviceInfo> DiscoverDevices(int maxDevices = byte.MaxValue)
        {
            List<BluetoothDeviceInfo> devices = new List<BluetoothDeviceInfo>();

            BLUETOOTH_DEVICE_SEARCH_PARAMS search = BLUETOOTH_DEVICE_SEARCH_PARAMS.Create();
            search.cTimeoutMultiplier = 8;
            search.fReturnAuthenticated = false;
            search.fReturnRemembered = true;
            search.fReturnUnknown = true;
            search.fReturnConnected = true;
            search.fIssueInquiry = true;

            BLUETOOTH_DEVICE_INFO device = BLUETOOTH_DEVICE_INFO.Create();
            IntPtr searchHandle = NativeMethods.BluetoothFindFirstDevice(ref search, ref device);
            if (searchHandle != IntPtr.Zero)
            {
                devices.Add(new BluetoothDeviceInfo(device));

                while (NativeMethods.BluetoothFindNextDevice(searchHandle, ref device) && devices.Count < maxDevices)
                {
                    devices.Add(new BluetoothDeviceInfo(device));
                }

                NativeMethods.BluetoothFindDeviceClose(searchHandle);
            }

            // get full paired devices list and remove those not recently seen (they were added to the results above regardless)
            search.fReturnAuthenticated = true;
            search.fReturnRemembered = true;
            search.fReturnUnknown = false;
            search.fReturnConnected = false;
            search.fIssueInquiry = false;

            searchHandle = NativeMethods.BluetoothFindFirstDevice(ref search, ref device);
            if (searchHandle != IntPtr.Zero)
            {
                if (device.LastSeen < DateTime.Now.AddMinutes(-1))
                {
                    devices.Remove(new BluetoothDeviceInfo(device));
                }

                while (NativeMethods.BluetoothFindNextDevice(searchHandle, ref device))
                {
                    if (device.LastSeen < DateTime.Now.AddMinutes(-1))
                    {
                        devices.Remove(new BluetoothDeviceInfo(device));
                    }
                }

                NativeMethods.BluetoothFindDeviceClose(searchHandle);
            }

            return devices.AsReadOnly();
        }

        /// <summary>
        /// Connects the client to a remote Bluetooth host using the specified Bluetooth address and service identifier.
        /// </summary>
        /// <param name="address">The BluetoothAddress of the remote host.</param>
        /// <param name="service">The Service Class Id of the service on the remote host.
        /// The standard Bluetooth service classes are provided on <see cref="BluetoothService"/>.</param>
        public void Connect(BluetoothAddress address, Guid service)
        {
            Connect(new BluetoothEndPoint(address, service));
        }

        /// <summary>
        /// Connects a client to a specified endpoint.
        /// </summary>
        /// <param name="remoteEP">A <see cref="BluetoothEndPoint"/> that represents the remote device.</param>
        public void Connect(BluetoothEndPoint remoteEP)
        {
            EnsureNotDisposed();
            if (remoteEP == null)
            {
                throw new ArgumentNullException("remoteEP");
            }

            Connect_StartAuthenticator(remoteEP);
            try
            {
                _socket.Connect(remoteEP);
            }
            finally
            {
                Connect_StopAuthenticator();
            }
        }

        private void Connect_StartAuthenticator(BluetoothEndPoint remoteEP)
        {
#if WinXP
            if (m_pinForConnect != null)
            {
                SetPin(remoteEP.Address, m_pinForConnect);
            }
#endif
        }

        /// <summary>
        /// Sets the PIN associated with the currently connected device.
        /// </summary>
        /// <param name="pin">PIN which must be composed of 1 to 16 ASCII characters.</param>
        /// <remarks>Assigning null (Nothing in VB) or an empty String will revoke the PIN.</remarks>
        public void SetPin(string pin)
        {
            if (!Connected)
            {
#if WinXP
                m_pinForConnect = pin;
#else
                SetPin(null, pin);
#endif
            }
            else
            {
                EndPoint rep = _socket.RemoteEndPoint;
                BluetoothAddress addr = BluetoothAddress.None;
                if (rep != null)
                    addr = ((BluetoothEndPoint)rep).Address;
                if (addr.IsNull)
                    throw new InvalidOperationException(
                        "The socket needs to be connected to detect the remote device"
                        + ", use the other SetPin method..");
                SetPin(addr, pin);
            }
        }

        /// <summary>
        /// Set or change the PIN to be used with a specific remote device.
        /// </summary>
        /// <param name="device">Address of Bluetooth device.</param>
        /// <param name="pin">PIN string consisting of 1 to 16 ASCII characters.</param>
        /// <remarks>Assigning null (Nothing in VB) or an empty String will revoke the PIN.</remarks>
        public void SetPin(BluetoothAddress device, string pin)
        {
            option.SetPin(device, pin);
        }

        private void Connect_StopAuthenticator()
        {
#if WinXP
            if (m_pinForConnect != null)
            {
                SetPin(BluetoothAddress.None, null);
            }
#endif
        }

        /// <summary>
        /// Closes the BluetoothClient and the underlying connection.
        /// </summary>
        /// <remarks>The Close method marks the instance as disposed and requests that the associated Socket close the Bluetooth connection</remarks>
        public void Close()
        {
            if (_socket is object && _socket.Connected)
                _socket.Close();
        }

        public bool Connected
        {
            get
            {
                if (_socket == null)
                    return false;

                return _socket.Connected;
            }
        }

        public SocketOption Option
        {
            get => option;
        }

        #region Available
        public int Available
        {
            get
            {
                EnsureNotDisposed();
                return _socket.Available;
            }
        }
        #endregion

        private Socket _socket;

        public Socket Socket
        {
            get { return _socket; }
            set { _socket = value; }

        }

        public string RemoteMachineName
        {
            get
            {
                if (Connected)
                {
                    BluetoothEndPoint remote = _socket.RemoteEndPoint as BluetoothEndPoint;
                    BLUETOOTH_DEVICE_INFO info = BLUETOOTH_DEVICE_INFO.Create();
                    info.Address = remote.Address;
                    NativeMethods.BluetoothGetDeviceInfo(IntPtr.Zero, ref info);
                    return info.szName;
                }

                return string.Empty;
            }
        }

        #region Get Stream
        private System.IO.Stream dataStream;


        public System.IO.Stream GetStream()
        {
            EnsureNotDisposed();
            if (!Socket.Connected)
            {
                throw new InvalidOperationException("The operation is not allowed on non-connected sockets.");
            }

            if (dataStream == null)
            {
                dataStream = new BluetoothStream(Socket, true);
            }

            return dataStream;
        }
        #endregion

        #region IDisposable Support
        private void EnsureNotDisposed()
        {
            if (isDisposed || (_socket == null))
                throw new ObjectDisposedException("BluetoothClient");
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        private bool isDisposed = false; // To detect redundant calls

        void Dispose(bool disposing)
        {
            if (!isDisposed)
            {
                if (disposing)
                {
                    IDisposable idStream = dataStream;
                    if (idStream != null)
                    {
                        //dispose the stream which will also close the socket
                        idStream.Dispose();
                    }
                    else
                    {
                        if (_socket != null)
                        {
                            _socket.Close();
                            _socket = null;
                        }
                    }
                }

                isDisposed = true;
            }
        }
        #endregion
    }
}
