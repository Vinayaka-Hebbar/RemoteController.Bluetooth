using System;
using System.Net.Sockets;

namespace RemoteController.Sockets
{
    public sealed class BluetoothStream : System.IO.Stream
    {
        private readonly Socket _socket;
        private readonly bool _ownsSocket;

        public BluetoothStream(Socket socket, bool ownsSocket)
        {
            if (socket is null)
                throw new ArgumentNullException(nameof(socket));

            _socket = socket;
            _ownsSocket = ownsSocket;
        }

        public override void Close()
        {
            if (_ownsSocket)
            {
                _socket.Close();
            }

            base.Close();
        }

        public bool DataAvailable
        {
            get
            {
                return _socket.Available > 0;
            }
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override long Length => _socket.Available;

        public override bool CanWrite => true;

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            return _socket.Receive(buffer, offset, count, SocketFlags.None);
        }

        public override long Seek(long offset, System.IO.SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _socket.Send(buffer, offset, count, SocketFlags.None);
        }
    }
}
