using ManagementService.Services;
using System.Threading;
using System.Windows;
using WebSocketSharp.Server;

namespace ManagementService
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        WebSocketServer wssv;
        public static MainWindow Instance { get; private set; }
        public MainWindow()
        {
            InitializeComponent();
        }

        public void RecieveInfo(string info)
        {
            log.Text += info + "\n";
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Instance = this;
            wssv = new WebSocketServer("ws://localhost:4200");
            wssv.AddWebSocketService<UserService>("/user");
            wssv.Start();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            wssv.Stop();
        }
    }
}
