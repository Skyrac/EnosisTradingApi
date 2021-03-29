using Messages.Enums;
using Messages.Models;
using Newtonsoft.Json;
using System;
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
                    Console.WriteLine(e.Data);
                };
                ws.OnClose += (sender, e) =>
                {
                    ws.Close();
                };

                ws.Connect();
                ws.Send(JsonConvert.SerializeObject(new CandleServiceSubscriptionMessage(EExchange.BinanceSpot, Binance.Net.Enums.KlineInterval.OneMinute, "BTCUSDT", "ETHUSDT")));
                Console.ReadKey();
            }
        }
    }
}
