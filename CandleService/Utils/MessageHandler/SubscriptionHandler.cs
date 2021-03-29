using CandleService.Services;
using Messages.Models;
using System;

namespace CandleService.Utils.MessageHandler
{
    public static class SubscriptionHandler
    {
        public static void HandleMessage(CandleServiceSubscriptionMessage message, Subscriber subscriber)
        {
            if(!Program.Exchanges.ContainsKey(message.Exchange))
            {
                Console.WriteLine("{0}: Exchange {1} not found in registered Exchanges!", message.Exchange);
                return;
            }
            var exchange = Program.Exchanges[message.Exchange];
            exchange.AddListener(message.Symbols, message.Interval, subscriber);
        }
    }
}
