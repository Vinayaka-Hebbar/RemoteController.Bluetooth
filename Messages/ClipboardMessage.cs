using System.Runtime.CompilerServices;
using System.Text;

namespace RemoteController.Messages
{
    public readonly struct ClipboardMessage : IMessage
    {
        public readonly string Data;

        public ClipboardMessage(string data)
        {
            Data = data;
        }

        public ClipboardMessage(IMessage packet)
        {
            Data = Encoding.Default.GetString(packet.GetBytes());
        }

        public MessageType Type => MessageType.Clipboard;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte[] GetBytes()
        {
            return GetBytes(Data);
        }

        public static unsafe byte[] GetBytes(string data)
        {
            var count = Encoding.Default.GetByteCount(data);
            var res = new byte[count + Message.HeaderSize];
            fixed (byte* b = res)
            {
                Message.SetHeader(b, Message.Clipboard, count);
                var bytes = b;
                // skip the header
                bytes += Message.HeaderSize;
                fixed (char* c = data)
                    Encoding.Default.GetBytes(c, count, bytes, res.Length);
            }
            return res;
        }
    }
}
