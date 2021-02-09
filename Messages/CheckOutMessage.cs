using System.Text;
using System.Threading.Tasks;

namespace RemoteController.Messages
{
    public readonly struct CheckOutMessage : IMessage
    {
        public readonly string ClientName;

        public MessageType Type => MessageType.CheckOut;

        public CheckOutMessage(string clientName)
        {
            ClientName = clientName;
        }

        public unsafe CheckOutMessage(IMessage message)
        {
            ClientName = Encoding.UTF8.GetString(message.GetBytes());
        }

        public unsafe byte[] GetBytes()
        {
            // header + 2 bytes for client size and 1 byte for num of screen
            int clientIdSize = Encoding.Default.GetByteCount(ClientName);
            int size = Message.HeaderSize + clientIdSize;
            var res = new byte[size];
            fixed (byte* b = res)
            {
                Message.SetHeader(b, Message.CheckOut, clientIdSize);
                var bytes = b;
                // skip the header
                bytes += Message.HeaderSize;
                fixed (char* id = ClientName)
                {
                    bytes += Encoding.Default.GetBytes(id, clientIdSize, bytes, size);
                }
            }
            return res;
        }

        public async static Task<CheckOutMessage> ParseAsync(MessageInfo info, System.IO.Stream stream)
        {
            var buffer = new byte[info.Length];
            if (await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken: System.Threading.CancellationToken.None) > 0)
            {
                return new CheckOutMessage(Encoding.UTF8.GetString(buffer));
            }
            return default;
        }
    }
}
