﻿using RemoteController.Core;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace RemoteController.Messages
{
    public readonly struct CheckInMessage : IMessage
    {
        public readonly string ClientName;

        public readonly IList<VirtualScreen> Screens;

        public CheckInMessage(string clientName, IList<VirtualScreen> screens)
        {
            ClientName = clientName;
            Screens = screens;
        }

        public MessageType Type => MessageType.CheckIn;

        public unsafe byte[] GetBytes()
        {
            // header + 2 bytes for client size and 1 byte for num of screen
            var size = 11;
            int clientIdSize = Encoding.Default.GetByteCount(ClientName);
            size += clientIdSize;
            var count = Screens.Count;
            size += count * 48;
            var res = new byte[size];
            fixed (byte* b = res)
            {
                Message.SetHeader(b, Message.CheckIn, size - 8);
                var bytes = b;
                // skip the header
                bytes += 8;
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
                    *(long*)bytes = (long)item.LocalX;
                    bytes += 8;
                    *(long*)bytes = (long)item.LocalY;
                    bytes += 8;

                    *(long*)bytes = (long)item.X;
                    bytes += 8;
                    *(long*)bytes = (long)item.Y;
                    bytes += 8;

                    *(long*)bytes = (long)item.Width;
                    bytes += 8;
                    *(long*)bytes = (long)item.Height;
                    bytes += 8;
                }
            }
            return res;
        }

        public unsafe static CheckInMessage Parse(MessageInfo info, NetworkStream stream)
        {
            var buffer = new byte[info.Length];
            if (stream.Read(buffer, 0, info.Length) > 0)
            {
                fixed (byte* b = buffer)
                {
                    var cu = b;
                    int clientIdSize = *(short*)b;
                    cu += 2;
                    var clientId = Encoding.Default.GetString(buffer, (int)(cu - b), clientIdSize);
                    cu += clientIdSize;
                    int screensCount = *cu;
                    cu++;
                    var screens = new VirtualScreen[screensCount];
                    for (int i = 0; i < screensCount; i++)
                    {
                        double localX = *(long*)cu;
                        cu += 8;
                        double localY = *(long*)cu;
                        cu += 8;
                        double x = *(long*)cu;
                        cu += 8;
                        double y = *(long*)cu;
                        cu += 8;
                        double width = *(long*)cu;
                        cu += 8;
                        double height = *(long*)cu;
                        cu += 8;
                        screens[i] = new VirtualScreen(clientId)
                        {
                            LocalX = localX,
                            LocalY = localY,
                            X = x,
                            Y = y,
                            Width = width,
                            Height = height
                        };
                    }
                    return new CheckInMessage(clientId, screens);
                }
            }
            return default;
        }
    }
}
