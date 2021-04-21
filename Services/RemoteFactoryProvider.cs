using RemoteController.Sockets;
using System.Net;

namespace RemoteController.Services
{
    public class RemoteFactoryProvider
    {
        public ISocketClient CreateClient()
        {
            return new SocketClient();
        }

        public ISocketListener CreateListener()
        {
            return new SocketListener(new IPEndPoint(IPAddress.Any, 40580));
        }
    }
}
