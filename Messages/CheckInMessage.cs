using RemoteController.Core;
using System.Collections.Generic;
using System.Text;

namespace RemoteController.Messages
{
    public readonly struct CheckInMessage : IMessage
    {
        public readonly string ClientName;

        public readonly IReadOnlyList<VirtualScreen> Screens;

        public CheckInMessage(string clientName, IReadOnlyList<VirtualScreen> screens)
        {
            ClientName = clientName;
            Screens = screens;
        }

        public unsafe CheckInMessage(IMessage message)
        {
            var buffer = message.GetBytes();
            fixed (byte* b = buffer)
            {
                var cu = b;
                // first byte is client Id Size
                int clientIdSize = *(short*)b;
                cu += 2;
                var clientId = Encoding.Default.GetString(buffer, (int)(cu - b), clientIdSize);
                cu += clientIdSize;
                int screensCount = *cu;
                cu++;
                var screens = new VirtualScreen[screensCount];
                for (int i = 0; i < screensCount; i++)
                {
                    int localX = *(int*)cu;
                    cu += 4;
                    int localY = *(int*)cu;
                    cu += 4;
                    int x = *(int*)cu;
                    cu += 4;
                    int y = *(int*)cu;
                    cu += 4;
                    int width = *(int*)cu;
                    cu += 4;
                    int height = *(int*)cu;
                    cu += 4;
                    int dpiX = *(int*)cu;
                    cu += 4;
                    int dpiY = *(int*)cu;
                    cu += 4;
                    screens[i] = new VirtualScreen(clientId, new Win32.Hooks.Dpi(dpiX, dpiY))
                    {
                        LocalX = localX,
                        LocalY = localY,
                        X = x,
                        Y = y,
                        Width = width,
                        Height = height
                    };
                }


                ClientName = clientId;
                Screens = screens;
            }
        }

        public MessageType Type => MessageType.CheckIn;

        public unsafe byte[] GetBytes()
        {
            // header + 2 bytes for client size and 1 byte for num of screen
            var size = 13;
            int clientIdSize = Encoding.Default.GetByteCount(ClientName);
            size += clientIdSize;
            var count = Screens.Count;
            // Single Virtual screen size
            size += count * 32;
            var res = new byte[size];
            fixed (byte* b = res)
            {
                Message.SetHeader(b, Message.CheckIn, size - Message.HeaderSize);
                var bytes = b;
                // skip the header
                bytes += Message.HeaderSize;
                *(short*)bytes = (short)clientIdSize;
                bytes += 2;
                fixed (char* id = ClientName)
                {
                    bytes += Encoding.Default.GetBytes(id, clientIdSize, bytes, size);
                }
                *bytes = (byte)count;
                bytes++;
                for (int i = 0; i < count; i++)
                {
                    VirtualScreen item = Screens[i];
                    *(int*)bytes = item.LocalX;
                    bytes += 4;
                    *(int*)bytes = item.LocalY;
                    bytes += 4;

                    *(int*)bytes = item.X;
                    bytes += 4;
                    *(int*)bytes = item.Y;
                    bytes += 4;

                    *(int*)bytes = item.Width;
                    bytes += 4;
                    *(int*)bytes = item.Height;
                    bytes += 4;
                    *(int*)bytes = item.Dpi.X;
                    bytes += 4;
                    *(int*)bytes = item.Dpi.Y;
                    bytes += 4;
                }
            }
            return res;
        }
    }
}
