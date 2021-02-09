using System;
using System.ComponentModel;
using System.Windows;

namespace RemoteController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Controls.ListViewDragDropManager<Model.DeviceScreens> dragDropManager;

        public MainWindow()
        {
            DataContext = App.MainModel;
            dragDropManager = new Controls.ListViewDragDropManager<Model.DeviceScreens>();
            InitializeComponent();
        }

        public ViewModels.MainViewModel ViewModel
        {
            get => (ViewModels.MainViewModel)GetValue(DataContextProperty);
        }

        internal static bool IsAdminUser
        {
            get
            {
                return new System.Security.Principal.WindowsPrincipal
                    (System.Security.Principal.WindowsIdentity.GetCurrent())
                    .IsInRole(System.Security.Principal.WindowsBuiltInRole.Administrator);
            }
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            dragDropManager.ListView = DeviceListView;
            await Dispatcher.InvokeAsync(UpdateTitle);
            await ViewModel.InitAsync();
        }

        void UpdateTitle()
        {
            if (IsAdminUser)
            {
                Title = "Remote Controller (Admin)";
            }
            else
            {
                Title = "Remote Controller";
            }
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ViewModels.MainViewModel vm = ViewModel;
            if (vm.Sender.IsConnected == false && vm.Receiver.IsConnected == false)
            {
                Application.Current.Shutdown();
            }
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);
            dragDropManager.ListView = null;
        }
    }
}
