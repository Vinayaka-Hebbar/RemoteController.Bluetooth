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

        public unsafe byte[] GetBytes()
        {
            var count = Encoding.Default.GetByteCount(Data);
            var res = new byte[count + Message.HeaderSize];
            fixed (byte* b = res)
            {
                Message.SetHeader(b, Message.Clipboard, count);
                var bytes = b;
                // skip the header
                bytes += Message.HeaderSize;
                fixed (char* c = Data)
                    Encoding.Default.GetBytes(c, count, bytes, res.Length);
            }
            return res;
        }
    }
}
