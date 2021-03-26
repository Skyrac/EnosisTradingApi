using System.Windows;
using System.Windows.Threading;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace ManagementService.Services
{
    public class UserService : WebSocketBehavior
    {

        protected override void OnOpen()
        {
            base.OnOpen();
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.Instance.RecieveInfo(string.Format("{0} connected to User Service.", Context.UserEndPoint));
            });
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            var msg = e.Data == "BALUS"
                      ? "I've been balused already..."
                      : "I'm not available now.";
            Application.Current.Dispatcher.Invoke(() =>
            {
                MainWindow.Instance.RecieveInfo(string.Format("{0} sent message: {1}.", Context.UserEndPoint, e.Data));
            });
            Send(msg);
        }
    }
}
