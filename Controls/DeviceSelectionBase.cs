using System.Windows.Controls;

namespace RemoteController.Controls
{
    public class DeviceSelectionBase : Grid
    {
        public virtual Model.IDeviceOption GetDeviceOption()
        {
            return null;
        }

        public virtual bool Validate()
        {
            return true;
        }
    }
}
