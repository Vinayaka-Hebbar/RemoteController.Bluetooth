using System;
using System.Net;
using System.Net.Sockets;

namespace RemoteController.Sockets
{
    public class SocketListener : ISocketListener
    {
        private bool active;
        private readonly EndPoint serverEP;
        public bool Active
        {
            get => active;
        }

        #region Server

        private Socket serverSocket;

        public SocketListener(EndPoint serverEP)
        {
            this.serverEP = serverEP;
            serverSocket = new Socket(serverEP.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        /// <summary>
        /// Gets the underlying network <see cref="Socket"/>.
        /// </summary>
        /// <value>The underlying <see cref="Socket"/>.</value>
        /// <remarks><see cref="WindowsBluetoothListener"/> creates a <see cref="Socket"/> to listen for incoming client connection requests.
        /// Classes deriving from <see cref="WindowsBluetoothListener"/> can use this property to get this <see cref="Socket"/>.
        /// Use the underlying <see cref="Socket"/> returned by the <see cref="Server"/> property if you require access beyond that which <see cref="WindowsBluetoothListener"/> provides.
        /// <para>Note <see cref="Server"/> property only returns the <see cref="Socket"/> used to listen for incoming client connection requests.
        /// Use the <see cref="AcceptSocket"/> method to accept a pending connection request and obtain a <see cref="Socket"/> for sending and receiving data.
        /// You can also use the <see cref="AcceptClient"/> method to accept a pending connection request and obtain a <see cref="BluetoothClient"/> for sending and receiving data.</para></remarks>
        public Socket Server
        {
            get
            {
                return serverSocket;
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
            }
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
		/// If your application is relatively simple, consider using the <see cref="AcceptClient"/> method rather than the AcceptSocket method.
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
        public ISocketClient AcceptClient()
        {
            Socket s = AcceptSocket();
            return new SocketClient(s);
        }


        #region Stop
        /// <summary>
        /// Stops the socket from monitoring connections.
        /// </summary>
        public void Stop()
        {
            if (serverSocket != null)
            {
                serverSocket.Close();
                serverSocket = null;
            }

            active = false;
            serverSocket = new BluetoothSocket();
        }
        #endregion

    }
}
