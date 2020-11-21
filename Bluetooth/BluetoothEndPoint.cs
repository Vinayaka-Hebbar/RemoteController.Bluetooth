using System;
using System.Net;
using System.Net.Sockets;

namespace RemoteController.Bluetooth
{
    public sealed class BluetoothEndPoint : EndPoint
    {
        private const int defaultPort = -1;
        private readonly ulong _bluetoothAddress;
        private readonly Guid _serviceId;
        private int m_port;

        internal BluetoothEndPoint(ulong bluetoothAddress, Guid serviceId) : this(bluetoothAddress, serviceId, defaultPort)
        {
        }

        internal BluetoothEndPoint(ulong bluetoothAddress, Guid serviceId, int port)
        {
            _bluetoothAddress = bluetoothAddress;
            _serviceId = serviceId;
            m_port = port;
        }

        internal BluetoothEndPoint(byte[] sockaddr_bt)
        {
            if (sockaddr_bt[0] != 32)
                throw new ArgumentException(nameof(sockaddr_bt));

            byte[] addrbytes = new byte[8];

            for (int ibyte = 0; ibyte < 8; ibyte++)
            {
                addrbytes[ibyte] = sockaddr_bt[2 + ibyte];
            }
            _bluetoothAddress = BitConverter.ToUInt64(addrbytes, 0);

            byte[] servicebytes = new byte[16];

            for (int ibyte = 0; ibyte < 16; ibyte++)
            {
                servicebytes[ibyte] = sockaddr_bt[10 + ibyte];
            }

            _serviceId = new Guid(servicebytes);
        }

        public override AddressFamily AddressFamily
        {
            get
            {
                return (AddressFamily)32;
            }
        }

        public BluetoothAddress Address
        {
            get
            {
                return _bluetoothAddress;
            }
        }

        /// <summary>
        /// Gets or sets the service channel number of the endpoint.
        /// </summary>
        public int Port
        {
            get { return m_port; }
            set { m_port = value; }
        }

        public Guid Service
        {
            get
            {
                return _serviceId;
            }
        }

        public override EndPoint Create(SocketAddress socketAddress)
        {
            if (socketAddress == null)
            {
                throw new ArgumentNullException("socketAddress");
            }

            if (socketAddress.Family == AddressFamily)
            {
                int ibyte;



                byte[] addrbytes = new byte[8];

                for (ibyte = 0; ibyte < 8; ibyte++)
                {
                    addrbytes[ibyte] = socketAddress[2 + ibyte];
                }
                ulong address = BitConverter.ToUInt64(addrbytes, 0);

                byte[] servicebytes = new byte[16];

                for (ibyte = 0; ibyte < 16; ibyte++)
                {
                    servicebytes[ibyte] = socketAddress[10 + ibyte];
                }

                byte[] portbytes = new byte[4];
                for (ibyte = 0; ibyte < 4; ibyte++)
                {
                    portbytes[ibyte] = socketAddress[26 + ibyte];
                }

                return new BluetoothEndPoint(address, new Guid(servicebytes), BitConverter.ToInt32(portbytes, 0));
            }

            return base.Create(socketAddress);
        }

        public override SocketAddress Serialize()
        {
            SocketAddress btsa = new SocketAddress(AddressFamily, 30);

            // copy address type
            btsa[0] = checked((byte)AddressFamily);

            // copy device id
            byte[] deviceidbytes = BitConverter.GetBytes(_bluetoothAddress);

            for (int idbyte = 0; idbyte < 6; idbyte++)
            {
                btsa[idbyte + 2] = deviceidbytes[idbyte];
            }

            // copy service clsid
            if (_serviceId != Guid.Empty)
            {
                byte[] servicebytes = _serviceId.ToByteArray();

                for (int servicebyte = 0; servicebyte < 16; servicebyte++)
                {
                    btsa[servicebyte + 10] = servicebytes[servicebyte];
                }
            }
            //copy port
            byte[] portbytes = BitConverter.GetBytes(m_port);
            for (int portbyte = 0; portbyte < 4; portbyte++)
            {
                btsa[portbyte + 26] = portbytes[portbyte];
            }

            return btsa;
        }

        public override int GetHashCode()
        {
            return _bluetoothAddress.GetHashCode();
        }

        public override string ToString()
        {
            return _bluetoothAddress.ToString("X6") + ":" + _serviceId.ToString("D");
        }
    }
}
