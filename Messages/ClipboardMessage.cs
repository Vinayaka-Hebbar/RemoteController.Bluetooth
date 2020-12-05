﻿using System.Net.Sockets;
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

        public MessageType Type => MessageType.Clipboard;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public unsafe byte[] GetBytes()
        {
            return GetBytes(Data);
        }

        public static unsafe byte[] GetBytes(string data)
        {
            var count = Encoding.Default.GetByteCount(data);
            var res = new byte[count + 8];
            fixed (byte* b = res)
            {
                Message.SetHeader(b, Message.Clipboard, count);
                var bytes = b;
                // skip the header
                bytes += 8;
                fixed (char* c = data)
                    Encoding.Default.GetBytes(c, count, bytes, res.Length);
            }
            return res;
        }

        public static unsafe ClipboardMessage Parse(MessageInfo info, NetworkStream stream)
        {
            var buffer = new byte[info.Length];
            if (stream.Read(buffer, 0, info.Length) > 0)
            {
                return new ClipboardMessage(Encoding.Default.GetString(buffer));
            }
            return default;
        }
    }
}
