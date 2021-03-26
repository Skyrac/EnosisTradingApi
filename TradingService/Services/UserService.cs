using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TradingService.Services
{
    public class UserService : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Console.WriteLine("{0} connected to User Service.", Context.UserEndPoint);
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            var msg = e.Data == "BALUS"
                      ? "I've been balused already..."
                      : "I'm not available now.";
            Console.WriteLine("{0} sent message: {1}.", Context.UserEndPoint, e.Data);
            Send(msg);
        }
    }
}
