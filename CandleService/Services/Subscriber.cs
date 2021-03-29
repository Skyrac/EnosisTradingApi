using CandleService.Utils.MessageHandler;
using Messages;
using Messages.Enums;
using Messages.Models;
using Newtonsoft.Json;
using System;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace CandleService.Services
{
    public class Subscriber : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            Console.WriteLine("{0} connected to User Service.", Context.UserEndPoint);
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            if(e.IsText && e.Data.Length > 0)
            {
                var message = JsonConvert.DeserializeObject<BaseMessage>(e.Data);
                if (message == null)
                {
                    Console.WriteLine("Unprovided message");
                    return;
                }
                HandleMessage(message.Type, e.Data);
            }
        }

        private void HandleMessage(EMessage type, string data)
        {
            Console.WriteLine("Recieved Message of Type {0}", type);
            switch(type)
            {
                case EMessage.Subscription:
                    SubscriptionHandler.HandleMessage(JsonConvert.DeserializeObject<CandleServiceSubscriptionMessage>(data), this);
                    break;
            }
        }

        protected override void OnClose(CloseEventArgs e)
        {
            foreach(var exchange in Program.Exchanges.Values)
            {
                exchange.Unsubscribe(this);
            }
        }
    }
}
