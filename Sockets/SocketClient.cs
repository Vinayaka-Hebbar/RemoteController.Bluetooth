using System;
using System.Net;
using System.Net.Sockets;

namespace RemoteController.Sockets
{
    public class SocketClient : ISocketClient
    {
        private Socket _socket;

        public SocketClient()
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public SocketClient(Socket socket)
        {
            _socket = socket;
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

        public Socket Socket
        {
            get { return _socket; }
            set { _socket = value; }
        }

        /// <summary>
        /// Connects a client to a specified endpoint.
        /// </summary>
        /// <param name="remoteEP">A <see cref="BluetoothEndPoint"/> that represents the remote device.</param>
        public void Connect(EndPoint remoteEP)
        {
            EnsureNotDisposed();
            if (remoteEP is null)
            {
                throw new ArgumentNullException("remoteEP");
            }

            try
            {
                _socket.Connect(remoteEP);
            }
            finally
            {
            }
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

        #region Get Stream
        private System.IO.Stream dataStream;


        public System.IO.Stream GetStream()
        {
            EnsureNotDisposed();
            if (!_socket.Connected)
            {
                throw new InvalidOperationException("The operation is not allowed on non-connected sockets.");
            }

            if (dataStream == null)
            {
                dataStream = new SocketStream(_socket, true);
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
            OnDispose();
            GC.SuppressFinalize(this);
        }

        ~SocketClient()
        {
            OnDispose();
        }

        private bool isDisposed = false; // To detect redundant calls

        void OnDispose()
        {
            if (!isDisposed)
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

                isDisposed = true;
            }
        }
        #endregion
    }
}
