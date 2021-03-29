using Newtonsoft.Json;
using System;
using Utils.Enums;
using Utils.Messages.Models;
using WebSocketSharp;

namespace TradingService
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ws = new WebSocket("ws://localhost:4300/subscribe"))
            {
                ws.OnMessage += (sender, e) =>
                {
                    if(e.IsText && e.Data.Length > 0)
                    {
                        var data = JsonConvert.DeserializeObject<CandleServiceUpdateMessage>(e.Data);
                        if(data == null || data.IntervalCandles.Count == 0)
                        {
                            return;
                        }
                        Console.WriteLine("Recieved Intervals: {0}", data.IntervalCandles.Count);
                        foreach(var intervalCandle in data.IntervalCandles)
                        {
                            Console.WriteLine("Interval {0} got {1} candles", intervalCandle.Interval, intervalCandle.Candles.Count);
                        }
                    }
                };
                ws.OnClose += (sender, e) =>
                {
                    ws.Close();
                };

                ws.Connect();
                ws.Send(JsonConvert.SerializeObject(new CandleServiceSubscriptionMessage(EExchange.BinanceSpot, Binance.Net.Enums.KlineInterval.OneMinute)));
                Console.ReadKey();
            }
        }
    }
}
