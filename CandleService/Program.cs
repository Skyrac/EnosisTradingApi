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
        private static WebSocketServer _server;
        public static Dictionary<EExchange, BaseExchange> Exchanges = new Dictionary<EExchange, BaseExchange>();
        static void Main(string[] args)
        {
            Exchanges.Add(EExchange.BinanceSpot, new BinanceExchange(EExchange.BinanceSpot));
            Exchanges.Add(EExchange.BinanceFuturesUsdt, new BinanceExchange(EExchange.BinanceFuturesUsdt));

            _server = new WebSocketServer("ws://localhost:4300");
            _server.AddWebSocketService<Subscriber>("/subscribe");
            _server.Start();
            Console.ReadKey();
        }
    }
}
