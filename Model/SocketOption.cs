using System.Net;

namespace RemoteController.Model
{
    public class SocketOption : IDeviceOption
    {
        private EndPoint endPoint;

        public RemoteDeviceType Type => RemoteDeviceType.Socket;

        public EndPoint EndPoint
        {
            get => endPoint;
            set => endPoint = value;
        }

        public EndPoint GetEndPoint()
        {
            return endPoint;
        }

        public override string ToString()
        {
            return endPoint?.ToString();
        }
    }
}
