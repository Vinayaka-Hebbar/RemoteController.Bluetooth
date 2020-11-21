using RemoteController.Bluetooth;
using System;
using System.Net;
using System.Net.Sockets;

namespace RemoteController.Sockets
{
    public class BluetoothListener
    {
        private bool active;
        private readonly BluetoothEndPoint serverEP;
        private readonly SocketOption option;


        private IntPtr serviceHandle;
        private ServiceRecord m_serviceRecord;
        private bool m_manualServiceRecord;
        private ServiceClass codService;
        private byte[] m_activeServiceRecordBytes; // As passed to WSASetService(REGISTER).

        #region Constructors
        public BluetoothListener(Guid service)
        {
            InitServiceRecord(service);
            serverEP = new BluetoothEndPoint(BluetoothAddress.None, service);
            serverSocket = new Socket(SocketFamily.Bluetooth, SocketType.Stream, BluetoothProtocolType.RFComm);
            option = new SocketOption(serverSocket);
        }

        public BluetoothListener(Guid service, ServiceRecord sdpRecord)
        {
            InitServiceRecord(sdpRecord);
            serverEP = new BluetoothEndPoint(BluetoothAddress.None, service);
            serverSocket = new Socket(SocketFamily.Bluetooth, SocketType.Stream, BluetoothProtocolType.RFComm);
            option = new SocketOption(serverSocket);
        }

        public BluetoothListener(Guid service, byte[] sdpRecord, int channelOffset)
        {
            InitServiceRecord(sdpRecord, channelOffset);
            serverEP = new BluetoothEndPoint(BluetoothAddress.None, service);
            serverSocket = new Socket(SocketFamily.Bluetooth, SocketType.Stream, BluetoothProtocolType.RFComm);
            option = new SocketOption(serverSocket);
        } 
        #endregion

        public SocketOption Option
        {
            get => option;
        }

        private ServiceRecord CreateBasicRfcommRecord(Guid serviceClassUuid, string svcName)
        {
            ServiceRecordBuilder bldr = new ServiceRecordBuilder();
            System.Diagnostics.Debug.Assert(bldr.ProtocolType == BluetoothProtocolDescriptorType.Rfcomm);
            bldr.AddServiceClass(serviceClassUuid);
            if (svcName != null)
            {
                bldr.ServiceName = svcName;
            }
            return bldr.ServiceRecord;
        }

        public bool Active
        {
            get => active;
        }

        #region Server

        private Socket serverSocket;

        /// <summary>
		/// Gets the underlying network <see cref="Socket"/>.
		/// </summary>
		/// <value>The underlying <see cref="Socket"/>.</value>
		/// <remarks><see cref="WindowsBluetoothListener"/> creates a <see cref="Socket"/> to listen for incoming client connection requests.
		/// Classes deriving from <see cref="WindowsBluetoothListener"/> can use this property to get this <see cref="Socket"/>.
		/// Use the underlying <see cref="Socket"/> returned by the <see cref="Server"/> property if you require access beyond that which <see cref="WindowsBluetoothListener"/> provides.
		/// <para>Note <see cref="Server"/> property only returns the <see cref="Socket"/> used to listen for incoming client connection requests.
		/// Use the <see cref="AcceptSocket"/> method to accept a pending connection request and obtain a <see cref="Socket"/> for sending and receiving data.
		/// You can also use the <see cref="AcceptBluetoothClient"/> method to accept a pending connection request and obtain a <see cref="BluetoothClient"/> for sending and receiving data.</para></remarks>
		public Socket Server
        {
            get
            {
                return serverSocket;
            }
        }
        #endregion

        #region Service Class
        /// <summary>
        /// Get or set the Service Class flags that this service adds to the host 
        /// device&#x2019;s Class Of Device field.
        /// </summary>
        /// -
        /// <remarks>
        /// <para>The Class of Device value contains a Device part which describes 
        /// the primary service that the device provides, and a Service part which 
        /// is a set of flags indicating all the service types that the device supports, 
        /// e.g. <see cref="ServiceClass.ObjectTransfer"/>,
        /// <see cref="ServiceClass.Telephony"/>,
        /// <see cref="ServiceClass.Audio"/> etc.
        /// This property supports setting those flags; bits set in this value will be 
        /// added to the host device&#x2019;s CoD Service Class bits when the listener
        /// is active.
        /// </para>
        /// <para><note>Supported on Win32, but not supported on WindowsMobile/WinCE 
        /// as there's no native API for it.  The WindowCE section of MSDN mentions the
        /// Registry value <c>COD</c> at key <c>HKEY_LOCAL_MACHINE\Software\Microsoft\Bluetooth\sys</c>. 
        /// However my (Jam) has value 0x920100 there but advertises a CoD of 0x100114, 
        /// so its not clear how the values relate to each other.
        /// </note>
        /// </para>
        /// </remarks>
        public ServiceClass ServiceClass
        {
            get
            {
                return codService;
            }
            set
            {
                codService = value;
            }
        }

        private string m_serviceName;

        /// <summary>
        /// Get or set the ServiceName the server will use in its SDP Record.
        /// </summary>
        /// -
        /// <value>A string representing the value to be used for the Service Name
        /// SDP Attribute.  Will be <see langword="null"/> if not specfied.
        /// </value>
        /// -
        /// <exception cref="T:System.InvalidOperationException">
        /// The listener is already started.
        /// <para>- or -</para>
        /// A custom Service Record was given at initialization time.  In that case 
        /// the ServiceName attribute should be added to that record.
        /// </exception>
        public string ServiceName
        {
            get { return m_serviceName; }
            set
            {
                if (active)
                {
                    throw new InvalidOperationException("Can not change ServiceName when started.");
                }
                if (m_manualServiceRecord)
                {
                    throw new InvalidOperationException("ServiceName may not be specified when a custom Service Record is being used.");
                }
                m_serviceName = value;
                InitServiceRecord(serverEP.Service);
            }
        }

        #endregion

        #region Start
        /// <summary>
        /// Starts listening for incoming connection requests.
        /// </summary>
        public void Start()
        {
            Start(int.MaxValue);
        }

        /// <summary>
        /// Starts listening for incoming connection requests with a maximum number of pending connection.
        /// </summary>
        /// <param name="backlog">The maximum length of the pending connections queue.</param>
        public void Start(int backlog)
        {
            if ((backlog > int.MaxValue) || (backlog < 0))
            {
                throw new ArgumentOutOfRangeException("backlog");
            }
            if (serverSocket == null)
            {
                throw new InvalidOperationException("The socket handle is not valid.");
            }
            if (!active)
            {
                serverSocket.Bind(serverEP);

                //Console.WriteLine("WinBtLsnr BEFORE listen(), lep: " + serverSocket.LocalEndPoint);
                serverSocket.Listen(backlog);
                active = true;

                // (Do this after Listen as BlueZ doesn't assign the port until then).
                byte channelNumber = (byte)((BluetoothEndPoint)serverSocket.LocalEndPoint).Port;
                //Console.WriteLine("WinBtLsnr: lep: " + serverSocket.LocalEndPoint);
                SetService(channelNumber);
            }
        }

        private void SetService(byte channelNumber)
        {
            byte[] rawRecord = GetServiceRecordBytes(channelNumber);
            MicrosoftSdpService.SetService(rawRecord, codService);
        }
        #endregion

        #region Stop
        /// <summary>
        /// Stops the socket from monitoring connections.
        /// </summary>
        public void Stop()
        {
            if (serverSocket != null)
            {
                try
                {
                    if (serviceHandle != IntPtr.Zero)
                    {
                        MicrosoftSdpService.RemoveService(serviceHandle, m_activeServiceRecordBytes);
                        serviceHandle = IntPtr.Zero;
                    }
                }
                finally
                {
                    serverSocket.Close();
                    serverSocket = null;
                }
            }

            active = false;
            serverSocket = new Socket(SocketFamily.Bluetooth, SocketType.Stream, BluetoothProtocolType.RFComm);
        }
        #endregion

        #region Accept

        #region Async Socket
        /// <summary>
        /// Begins an asynchronous operation to accept an incoming connection attempt.
        /// </summary>
        /// <param name="callback">An <see cref="AsyncCallback"/> delegate that references the method to invoke when the operation is complete.</param>
        /// <param name="state">A user-defined object containing information about the accept operation.
        /// This object is passed to the callback delegate when the operation is complete.</param>
        /// <returns>An <see cref="IAsyncResult"/> that references the asynchronous creation of the <see cref="Socket"/>.</returns>
        /// <exception cref="ObjectDisposedException">The <see cref="Socket"/> has been closed.</exception>
        public IAsyncResult BeginAcceptSocket(AsyncCallback callback, object state)
        {
            if (!active)
            {
                throw new InvalidOperationException("Not listening. You must call the Start() method before calling this method.");
            }
            return serverSocket.BeginAccept(callback, state);
        }

        /// <summary>
        /// Asynchronously accepts an incoming connection attempt and creates a new <see cref="Socket"/> to handle remote host communication.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> returned by a call to the <see cref="BeginAcceptSocket"/> method.</param>
        /// <returns>A <see cref="Socket"/>.</returns>
        public Socket EndAcceptSocket(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }
            return serverSocket.EndAccept(asyncResult);
        }
        #endregion

        #region Async Client
        /// <summary>
		/// Begins an asynchronous operation to accept an incoming connection attempt.
		/// </summary>
		/// <param name="callback"></param>
		/// <param name="state"></param>
		/// <returns></returns>
		public IAsyncResult BeginAcceptBluetoothClient(AsyncCallback callback, object state)
        {
            if (!active)
            {
                throw new InvalidOperationException("Not listening. You must call the Start() method before calling this method.");
            }

            return serverSocket.BeginAccept(callback, state);
        }

        /// <summary>
        /// Asynchronously accepts an incoming connection attempt and creates a new <see cref="BluetoothClient"/> to handle remote host communication.
        /// </summary>
        /// <param name="asyncResult">An <see cref="IAsyncResult"/> returned by a call to the <see cref="BeginAcceptBluetoothClient"/> method.</param>
        /// <returns>A <see cref="BluetoothClient"/>.</returns>
        public BluetoothClient EndAcceptBluetoothClient(IAsyncResult asyncResult)
        {
            if (asyncResult == null)
            {
                throw new ArgumentNullException("asyncResult");
            }

            Socket s = serverSocket.EndAccept(asyncResult);
            return new BluetoothClient(s);
        }
        #endregion

        /// <summary>
		/// Creates a new socket for a connection.
		/// </summary>
		/// <remarks>AcceptSocket is a blocking method that returns a <see cref="Socket"/> that you can use to send and receive data.
		/// If you want to avoid blocking, use the <see cref="Pending"/> method to determine if connection requests are available in the incoming connection queue.
		/// <para>The <see cref="Socket"/> returned is initialized with the address and channel number of the remote device.
		/// You can use any of the Send and Receive methods available in the <see cref="Socket"/> class to communicate with the remote device.
		/// When you are finished using the <see cref="Socket"/>, be sure to call its <see cref="Socket.Close()"/> method.
		/// If your application is relatively simple, consider using the <see cref="AcceptBluetoothClient"/> method rather than the AcceptSocket method.
		/// <see cref="BluetoothClient"/> provides you with simple methods for sending and receiving data over a network in blocking synchronous mode.</para></remarks>
		/// <returns>A <see cref="Socket"/> used to send and receive data.</returns>
		/// <exception cref="InvalidOperationException">Listener is stopped.</exception>
		public Socket AcceptSocket()
        {
            if (!active)
            {
                throw new InvalidOperationException("Not listening. You must call the Start() method before calling this method.");
            }

            return serverSocket.Accept();
        }

        /// <summary>
        /// Creates a client object for a connection when the specified service or endpoint is detected by the listener component.
        /// </summary>
        /// <remarks>AcceptTcpClient is a blocking method that returns a <see cref="BluetoothClient"/> that you can use to send and receive data.
        /// Use the <see cref="Pending"/> method to determine if connection requests are available in the incoming connection queue if you want to avoid blocking.
        /// <para>Use the <see cref="BluetoothClient.GetStream"/> method to obtain the underlying <see cref="NetworkStream"/> of the returned <see cref="BluetoothClient"/>.
        /// The <see cref="NetworkStream"/> will provide you with methods for sending and receiving with the remote host.
        /// When you are through with the <see cref="BluetoothClient"/>, be sure to call its <see cref="BluetoothClient.Close"/> method.
        /// If you want greater flexibility than a <see cref="BluetoothClient"/> offers, consider using <see cref="AcceptSocket"/>.</para></remarks>
        /// <returns>A <see cref="BluetoothClient"/> component.</returns>
        /// <exception cref="InvalidOperationException">Listener is stopped.</exception>
        public BluetoothClient AcceptBluetoothClient()
        {
            Socket s = AcceptSocket();
            return new BluetoothClient(s);
        }
        #endregion

        #region Pending
        /// <summary>
        /// Determines if there is a connection pending.
        /// </summary>
        /// <returns>true if there is a connection pending; otherwise, false.</returns>
        public bool Pending()
        {
            if (!active)
            {
                throw new InvalidOperationException("Not listening. You must call the Start() method before calling this method.");
            }

            return serverSocket.Poll(0, SelectMode.SelectRead);
        }
        #endregion

        #region Host To Network Order
        internal static Guid HostToNetworkOrder(Guid hostGuid)
        {
            byte[] guidBytes = hostGuid.ToByteArray();

            BitConverter.GetBytes(IPAddress.HostToNetworkOrder(BitConverter.ToInt32(guidBytes, 0))).CopyTo(guidBytes, 0);
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder(BitConverter.ToInt16(guidBytes, 4))).CopyTo(guidBytes, 4);
            BitConverter.GetBytes(IPAddress.HostToNetworkOrder(BitConverter.ToInt16(guidBytes, 6))).CopyTo(guidBytes, 6);

            return new Guid(guidBytes);
        }
        #endregion

        #region Service Record
        /// <summary>
        /// Returns the SDP Service Record for this service.
        /// </summary>
        /// <remarks>
        /// <note>Returns <see langword="null"/> if the listener is not 
        /// <see cref="M:InTheHand.Net.Sockets.WindowsBluetoothListener.Start"/>ed
        /// (and an record wasn&#x2019;t supplied at initialization).
        /// </note>
        /// </remarks>
        public ServiceRecord ServiceRecord
        {
            get
            {
                return m_serviceRecord;
            }
        }


        private void InitServiceRecord(Guid serviceClassUuid)
        {
            m_serviceRecord = CreateBasicRfcommRecord(serviceClassUuid, m_serviceName);
        }

        private void InitServiceRecord(ServiceRecord sdpRecord)
        {
            if (sdpRecord == null)
            {
                throw new ArgumentNullException("sdpRecord");
            }
            if (ServiceRecordHelper.GetRfcommChannelNumber(sdpRecord) == -1)
            {
                throw new ArgumentException("The ServiceRecord must contain a RFCOMM-style ProtocolDescriptorList.");
            }
            m_serviceRecord = sdpRecord;
            m_manualServiceRecord = true;
        }

        private void InitServiceRecord(byte[] sdpRecord, int channelOffset)
        {
            if (sdpRecord.Length == 0)
            { throw new ArgumentException("sdpRecord must not be empty."); }
            if (channelOffset >= sdpRecord.Length)
            { throw new ArgumentOutOfRangeException("channelOffset"); }
            //
            // Parse into a ServiceRecord, and discard the array and offset!
            m_serviceRecord = ServiceRecord.CreateServiceRecordFromBytes(sdpRecord);
            m_manualServiceRecord = true;
        }

        // Called at registration time
        private byte[] GetServiceRecordBytes(byte channelNumber)
        {
            ServiceRecord record = m_serviceRecord;
            ServiceRecordHelper.SetRfcommChannelNumber(record, channelNumber);
            m_activeServiceRecordBytes = record.ToByteArray();
            System.Diagnostics.Debug.Assert(m_activeServiceRecordBytes != null);
            return m_activeServiceRecordBytes;
        }
        #endregion
    }
}
