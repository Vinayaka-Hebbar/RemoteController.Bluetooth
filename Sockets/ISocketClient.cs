using System.IO;
using System.Net;
using System.Net.Sockets;

namespace RemoteController.Sockets
{
    public interface ISocketClient : System.IDisposable
    {
        bool Connected { get; }
        Socket Socket { get; set; }

        void Close();
        void Connect(EndPoint remoteEP);
        Stream GetStream();
    }
}