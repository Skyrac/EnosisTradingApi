using CandleService.Exchanges;
using CandleService.Services;
using Utils.Enums;
using System;
using System.Collections.Generic;
using WebSocketSharp.Server;

namespace CandleService
{
    public class Program
    {
        public static WebSocketServer Server;
        public static Dictionary<EExchange, BaseExchange> Exchanges = new Dictionary<EExchange, BaseExchange>();
        static void Main(string[] args)
        {
            Exchanges.Add(EExchange.BinanceSpot, new BinanceExchange(EExchange.BinanceSpot));
            Exchanges.Add(EExchange.BinanceFuturesUsdt, new BinanceExchange(EExchange.BinanceFuturesUsdt));

            Server = new WebSocketServer("ws://localhost:4300");
            Server.AddWebSocketService<Subscriber>("/subscribe");
            Server.Start();
            Console.ReadKey();
        }
    }
}
