using System.Windows;

namespace RemoteController
{
    /// <summary>
    /// Interaction logic for Test.xaml
    /// </summary>
    public partial class Test : Window
    {
        private Controls.ListViewDragDropManager<Model.DeviceScreens> dragDropManager;
        public Test()
        {
            InitializeComponent();
            ItemsList.ItemsSource = new ItemSource
            {
                new Model.DeviceScreens()
                .AddScreen(new Win32.Hooks.Display(200,200))
                .AddScreen(new Win32.Hooks.Display(250,200)),
                new Model.DeviceScreens()
                .AddScreen(new Win32.Hooks.Display(300,200))
                .AddScreen(new Win32.Hooks.Display(250,200)),
                new Model.DeviceScreens("DESKTOP-DONO3911")
                .AddScreen(new Win32.Hooks.Display(500,200))
                .AddScreen(new Win32.Hooks.Display(250,200)),
            };
            dragDropManager = new Controls.ListViewDragDropManager<Model.DeviceScreens>(ItemsList);
        }

        public class ItemSource : System.Collections.ObjectModel.ObservableCollection<Model.DeviceScreens>, Collection.IObservableCollection<Model.DeviceScreens>
        {

        }
    }
}
