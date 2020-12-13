using RemoteController.Win32.Hooks;
using System.Collections.Generic;

namespace RemoteController.Model
{
    [System.Windows.Markup.ContentProperty("Displays")]
    public class DeviceScreens
    {
        public DeviceScreens() : this(System.Environment.MachineName)
        {
        }

        public DeviceScreens(string device)
        {
            Device = device;
            Displays = new System.Collections.ObjectModel.ObservableCollection<Display>();
        }

        public string Device { get; }

        public IList<Display> Displays { get; set; }

        public DeviceScreens AddScreen(Display display)
        {
            Displays.Add(display);
            return this;
        }
    }
}
