using RemoteController.Model;

namespace RemoteController.Services
{
    public class ApplicationSettings
    {
        private RemoteDeviceType deviceType;

        public RemoteDeviceType DeviceType
        {
            get => deviceType;
            set => deviceType = value;
        }
    }
}
