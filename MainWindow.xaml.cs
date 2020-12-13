using System;
using System.Windows;

namespace RemoteController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public ViewModels.MainViewModel ViewModel
        {
            get => (ViewModels.MainViewModel)GetValue(DataContextProperty);
        }

        public MainWindow()
        {
            DataContext = App.MainModel;
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            await ViewModel.InitAsync();
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
    }
}
