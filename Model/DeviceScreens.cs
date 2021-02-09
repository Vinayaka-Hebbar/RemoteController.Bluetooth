using System;
using System.Collections.Generic;

namespace RemoteController.Model
{
    [System.Windows.Markup.ContentProperty("Displays")]
    public class DeviceScreens : System.Windows.DependencyObject
    {
        public DeviceScreens() : this(System.Environment.MachineName)
        {
        }

        public DeviceScreens(string device)
        {
            Device = device;
            Displays = new System.Collections.ObjectModel.ObservableCollection<Core.IDisplay>();
        }

        public string Device { get; }

        public IList<Core.IDisplay> Displays { get; set; }

        public DeviceScreens AddScreen(Core.IDisplay display)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Action<Core.IDisplay>)Displays.Add, display);
            return this;
        }

        public void RemoveScreen(Core.IDisplay display)
        {
            Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, (Func<Core.IDisplay, bool>)Displays.Remove, display);
        }
    }
}
