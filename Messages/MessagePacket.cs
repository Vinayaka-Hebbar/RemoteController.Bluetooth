using System.Net.Sockets;
using System.Threading.Tasks;

namespace RemoteController.Messages
{
    public readonly struct MessagePacket : IMessage
    {
        public readonly MessageInfo Info;
        public readonly byte[] Data;

        public MessagePacket(MessageInfo info, byte[] data)
        {
            Info = info;
            Data = data;
        }

        public MessageType Type => Info.Type;

        public static MessagePacket Parse(MessageInfo info, System.IO.Stream stream)
        {
            var buffer = new byte[info.Length];
            if (stream.Read(buffer, 0, info.Length) > 0)
            {
                return new MessagePacket(info, buffer);
            }
            return default;
        }


        public static async Task<MessagePacket> ParseAsync(MessageInfo info, System.IO.Stream stream)
        {
            var buffer = new byte[info.Length];
            if (await stream.ReadAsync(buffer, 0, info.Length, cancellationToken: System.Threading.CancellationToken.None) > 0)
            {
                return new MessagePacket(info, buffer);
            }
            return default;
        }

        public byte[] GetBytes()
        {
            return Data;
        }
    }
}
