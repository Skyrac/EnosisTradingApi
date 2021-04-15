using Binance.Net.Enums;
using CandleService.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Utils.Candles.Models;
using Utils.Messages.Models;

namespace CandleService.Utils.MessageHandler
{
    public static class SubscriptionHandler
    {

        private static object _key = new Object();
        public static void HandleMessage(CandleServiceSubscriptionMessage message, Subscriber subscriber)
        {
            if(!Program.Exchanges.ContainsKey(message.Exchange))
            {
                Console.WriteLine("{0}: Exchange {1} not found in registered Exchanges!", message.Exchange);
                return;
            }
            var exchange = Program.Exchanges[message.Exchange];
            new Task(async() => {
                await exchange.AddListener(message.SubscriptionItems, subscriber, message.RequiredCandles);
                var tasks = new List<Task<KeyValuePair<KlineInterval, KeyValuePair<string, List<Kline>>>>>();
                foreach (var intervalItem in message.SubscriptionItems)
                {
                    foreach (var symbolItem in intervalItem.Candles)
                    {
                        tasks.Add(exchange.GetKlinesAsync(symbolItem.Symbol, intervalItem.Interval, message.RequiredCandles, null, null));
                    }
                    System.Threading.Thread.Sleep(300);
                }
                await Task.WhenAll(tasks);
                var candles = new List<IntervalSymbols>();
                Console.WriteLine("RECIEVED ALL REQUIRED CANDLES");
                foreach (var task in tasks)
                {
                    var result = task.Result;
                    if (!task.IsCompleted)
                    {
                        continue;
                    }
                    var intervalSymbol = candles.FirstOrDefault(item => item.Interval == result.Key);
                    if (intervalSymbol != default)
                    {
                        intervalSymbol.AddSymbol(result.Value);
                    }
                    else
                    {
                        candles.Add(new IntervalSymbols(result.Key, result.Value));
                    }
                }
                Console.WriteLine("GOT {0} Intervals", candles.Count);
                foreach (var interval in candles)
                {
                    Console.WriteLine("Interval {0} got {1} symbols", interval.Interval, interval.Symbols.Count);
                }
                subscriber.Context.WebSocket.Send(JsonConvert.SerializeObject(new CandleServiceHistoryCandlesMessage(candles)));
            }).Start();
        }
    }
}
