using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace RemoteController.Extensions
{
#if NETFRAMEWORK
    public static class SocketExtensions
    {
        public static Task<bool> ConnectAsync(this Socket self, EndPoint endPoint, int timeout)
        {
            return Task.Run(() =>
            {
                var clientDone = new System.Threading.ManualResetEvent(false);
                var arg = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = endPoint
                };
                var connected = false;
                arg.Completed += (s, e) =>
                {
                    connected = e.SocketError == SocketError.Success;
                    clientDone.Set();
                };
                self.ConnectAsync(arg);
                clientDone.Reset();
                clientDone.WaitOne(timeout);
                return connected;
            });
        }
    }
#endif
}
