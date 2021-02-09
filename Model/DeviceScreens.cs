using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace RemoteController.Model
{
    [System.Windows.Markup.ContentProperty("Displays")]
    public class DeviceScreens
    {
        private readonly string name;

        public DeviceScreens() : this(Environment.MachineName)
        {
        }

        public DeviceScreens(string name)
        {
            this.name = name;
            Displays = new System.Collections.ObjectModel.ObservableCollection<Core.IDisplay>();
        }

        public bool IsEmpty => Displays.Count == 0;

        public string Name => name;

        public IList<Core.IDisplay> Displays { get; set; }

        public DeviceScreens AddScreen(Core.IDisplay display)
        {
            InternalAdd(display);
            return this;
        }

        public async Task<bool> RemoveScreenAsync(Core.IDisplay display)
        {
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                return Displays.Remove(display);
            }
            else
            {
                return await dispatcher.InvokeAsync(new Func<bool>(() => Displays.Remove(display)), System.Windows.Threading.DispatcherPriority.Normal);
            }
        }

        void InternalAdd(Core.IDisplay item)
        {
            var dispatcher = Application.Current.Dispatcher;
            if (dispatcher.CheckAccess())
            {
                Displays.Add(item);
            }
            else
            {
                dispatcher.BeginInvoke(System.Windows.Threading.DispatcherPriority.Normal, new Action(() => Displays.Add(item)));
            }
        }



        public static string GetDeviceName(DeviceScreens device) => device.name;
    }
}
