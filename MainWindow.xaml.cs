using System;
using System.Windows;

namespace RemoteController
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ViewModels.MainViewModel vm;
        public MainWindow()
        {
            DataContext = vm = new ViewModels.MainViewModel();
            InitializeComponent();
        }

        protected override async void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            await vm.InitAsync();
        }
    }
}
