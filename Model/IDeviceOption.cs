using System.Net;

namespace RemoteController.Model
{
    public interface IDeviceOption
    {
        RemoteDeviceType Type { get; }
        EndPoint GetEndPoint();
        string ToString();
    }
}